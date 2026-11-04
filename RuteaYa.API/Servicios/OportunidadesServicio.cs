using Microsoft.EntityFrameworkCore;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.API.Servicios.Rutas;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Rutas;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    // Motor de compatibilidad: decide si un envío entra en un viaje (dueño, estado, fecha/día,
    // capacidad, peso y desvío) y con qué datos. Lo usan Oportunidades, Detalle, Aceptar y el Optimizador.
    public class OportunidadesServicio : IOportunidadesServicio
    {
        // Desvío / desvío máximo del viaje hasta el cual un envío es "Muy compatible" o "Compatible".
        private const decimal UmbralMuyCompatible = 0.3m;
        private const decimal UmbralCompatible = 0.5m;

        // El prefiltro en línea recta deja pasar hasta 1,5 veces el desvío máximo: por calle el desvío
        // puede ser distinto y no queremos descartar de más antes de consultar el servicio de rutas.
        private const decimal MargenPrefiltro = 1.5m;

        private static readonly TimeSpan _offsetArgentina = TimeSpan.FromHours(-3);

        private readonly IViajeRepositorio _viajeRepositorio;
        private readonly IEnvioRepositorio _envioRepositorio;
        private readonly IAsignacionRepositorio _asignacionRepositorio;
        private readonly IRutaServicio _rutaServicio;
        private readonly ILogger<OportunidadesServicio> _logger;

        public OportunidadesServicio(
            IViajeRepositorio viajeRepositorio,
            IEnvioRepositorio envioRepositorio,
            IAsignacionRepositorio asignacionRepositorio,
            IRutaServicio rutaServicio,
            ILogger<OportunidadesServicio> logger)
        {
            _viajeRepositorio = viajeRepositorio;
            _envioRepositorio = envioRepositorio;
            _asignacionRepositorio = asignacionRepositorio;
            _rutaServicio = rutaServicio;
            _logger = logger;
        }

        public async Task<OportunidadesViajeDto?> ListarAsync(int usuarioId, int viajeId, string? ordenarPor = "ganancia")
        {
            var viaje = await ObtenerViajePropioAsync(usuarioId, viajeId);
            if (viaje == null)
            {
                return null;
            }

            var resultado = new OportunidadesViajeDto
            {
                ViajeId = viaje.Id,
                OrigenDir = viaje.OrigenDir,
                DestinoDir = viaje.DestinoDir,
                Activo = viaje.Activo,
                MaxPaquetes = viaje.MaxPaquetes,
                PesoMaxKg = viaje.PesoMaxKg,
                DesvioMaxKm = viaje.DesvioMaxKm
            };

            var (paquetesLibres, kgLibres) = CapacidadRestante(viaje);
            resultado.PaquetesDisponibles = paquetesLibres;
            resultado.KgDisponibles = kgLibres;

            if (!viaje.Activo)
            {
                return resultado;
            }

            var hoy = Hoy();
            var envios = await _envioRepositorio.ListarDisponiblesAsync(hoy, usuarioId);
            var oportunidades = new List<OportunidadDto>();

            foreach (var envio in envios)
            {
                var evaluacion = await EvaluarInternoAsync(viaje, envio, usuarioId, hoy, prefiltrar: true);
                if (evaluacion.Motivo != null)
                {
                    continue;
                }

                var fecha = evaluacion.Fecha!.Value;
                var (compatibilidad, puntuacion) = CalcularCompatibilidad(evaluacion.Ruta!.DesvioKm, viaje.DesvioMaxKm);

                oportunidades.Add(new OportunidadDto
                {
                    EnvioId = envio.Id,
                    Origen = envio.OrigenDir,
                    Destino = envio.DestinoDir,
                    Descripcion = envio.Descripcion,
                    PesoKg = envio.PesoKg,
                    DesvioKm = evaluacion.Ruta.DesvioKm,
                    MontoPago = envio.Monto,
                    FechaViaje = fecha,
                    Compatibilidad = compatibilidad,
                    PuntuacionCompatibilidad = puntuacion,
                    CapacidadRestantePaquetes = paquetesLibres,
                    CapacidadRestanteKg = kgLibres
                });
            }

            resultado.Oportunidades = Ordenar(oportunidades, ordenarPor);
            return resultado;
        }

        public async Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int usuarioId, int viajeId, int envioId)
        {
            var viaje = await ObtenerViajePropioAsync(usuarioId, viajeId);
            if (viaje == null)
            {
                return null;
            }

            var envio = await _envioRepositorio.ObtenerPorIdAsync(envioId);
            if (envio == null || envio.UsuarioId == usuarioId || !EstaDisponible(envio))
            {
                return null;
            }

            var hoy = Hoy();
            var evaluacion = await EvaluarInternoAsync(viaje, envio, usuarioId, hoy, prefiltrar: false);

            var (paquetesRestantes, kgRestantes) = CapacidadRestante(viaje);

            var ruta = evaluacion.Ruta ?? new DetalleOportunidadRutaDto();
            ruta.MontoPago = envio.Monto;
            ruta.PesoKg = envio.PesoKg;
            ruta.GananciaPorKmAdicional = ruta.DesvioKm > 0 ? Math.Round(envio.Monto / ruta.DesvioKm, 2) : 0;
            // Lo que le quedaría al viaje después de aceptar este envío.
            ruta.CapacidadRestantePaquetes = paquetesRestantes - 1;
            ruta.CapacidadRestanteKg = kgRestantes - envio.PesoKg;

            return new DetalleOportunidadDto
            {
                EnvioId = envio.Id,
                ViajeId = viaje.Id,
                ViajeOrigenDir = viaje.OrigenDir,
                ViajeDestinoDir = viaje.DestinoDir,
                Envio = new EnvioDto
                {
                    Id = envio.Id,
                    OrigenDir = envio.OrigenDir,
                    DestinoDir = envio.DestinoDir,
                    FechaLimite = envio.FechaLimite,
                    PesoKg = envio.PesoKg,
                    Tamano = envio.Tamano,
                    Descripcion = envio.Descripcion,
                    Monto = envio.Monto,
                    Estado = envio.Estado
                },
                RutaDetalle = ruta,
                FechaViaje = evaluacion.Fecha,
                Compatibilidad = evaluacion.Ruta != null ? CalcularCompatibilidad(ruta.DesvioKm, viaje.DesvioMaxKm).Texto : string.Empty,
                PuedeAceptar = evaluacion.Motivo == null,
                Motivo = evaluacion.Motivo
            };
        }

        public async Task<ResultadoAsignacion> AceptarAsync(int usuarioId, int viajeId, int envioId)
        {
            return await AceptarVariosAsync(usuarioId, viajeId, new List<int> { envioId });
        }

        public async Task<ResumenSeleccionDto?> ResumirSeleccionAsync(int usuarioId, int viajeId, List<int> envioIds)
        {
            var viaje = await ObtenerViajePropioAsync(usuarioId, viajeId);
            if (viaje == null)
            {
                return null;
            }

            var seleccion = await EvaluarSeleccionAsync(viaje, envioIds, usuarioId);
            if (seleccion == null)
            {
                return null;
            }

            var envios = seleccion.Items
                .Select(i => ADtoSeleccion(i, viaje))
                .ToList();

            return new ResumenSeleccionDto
            {
                ViajeId = viaje.Id,
                OrigenDir = viaje.OrigenDir,
                DestinoDir = viaje.DestinoDir,
                MaxPaquetes = viaje.MaxPaquetes,
                PesoMaxKg = viaje.PesoMaxKg,
                DesvioMaxKm = viaje.DesvioMaxKm,
                Seleccion = new CombinacionDto
                {
                    Envios = envios,
                    GananciaTotal = envios.Sum(e => e.MontoPago),
                    PesoTotalKg = envios.Sum(e => e.PesoKg),
                    DesvioTotalKm = envios.Sum(e => e.DesvioKm),
                    CantidadPaquetes = envios.Count
                },
                PuedeConfirmar = seleccion.Motivo == null,
                Motivo = seleccion.Motivo
            };
        }

        public async Task<ResultadoAsignacion> AceptarVariosAsync(int usuarioId, int viajeId, List<int> envioIds)
        {
            var viaje = await ObtenerViajePropioAsync(usuarioId, viajeId);
            if (viaje == null)
            {
                return ResultadoAsignacion.NoEncontrado();
            }

            // Se revalida todo: entre que el conductor vio la selección y la confirmó pudo cambiar algo.
            var seleccion = await EvaluarSeleccionAsync(viaje, envioIds, usuarioId);
            if (seleccion == null)
            {
                return ResultadoAsignacion.NoEncontrado();
            }

            if (seleccion.Motivo != null)
            {
                return ResultadoAsignacion.Conflicto(seleccion.Motivo);
            }

            if (!seleccion.Items.All(i => EnvioEstadoMaquina.PuedeTransicionar(i.Envio.Estado, EstadoEnvio.ConductorAsignado)))
            {
                return ResultadoAsignacion.Conflicto(MotivoNoDisponible);
            }

            var asignaciones = new List<Asignacion>();
            foreach (var item in seleccion.Items)
            {
                EnvioEstadoMaquina.Transicionar(item.Envio, EstadoEnvio.ConductorAsignado);
                asignaciones.Add(new Asignacion
                {
                    ViajeId = viaje.Id,
                    EnvioId = item.Envio.Id,
                    FechaViaje = item.Evaluacion.Fecha!.Value,
                    DesvioKm = item.Evaluacion.Ruta!.DesvioKm,
                    TiempoExtraMin = item.Evaluacion.Ruta.TiempoAdicionalMinutos,
                    Ganancia = item.Envio.Monto,
                    AsignadoEn = DateTime.UtcNow,
                    Envio = item.Envio
                });
            }

            try
            {
                // Un solo SaveChanges guarda todas las asignaciones y los cambios de estado (con su historial).
                await _asignacionRepositorio.AgregarAsync(asignaciones);
            }
            catch (DbUpdateException ex)
            {
                // El índice único sobre EnvioId: otro conductor tomó alguno de los envíos al mismo tiempo.
                _logger.LogInformation(ex, "Alguno de los envíos {EnvioIds} ya fue asignado por otro conductor.", string.Join(",", envioIds));
                return ResultadoAsignacion.Conflicto(asignaciones.Count == 1
                    ? "Otro conductor acaba de tomar este envío."
                    : "Otro conductor acaba de tomar uno de los envíos.");
            }

            return ResultadoAsignacion.Exito(asignaciones.Select(a => new AsignacionDto
            {
                Id = a.Id,
                EnvioId = a.EnvioId,
                ViajeId = a.ViajeId,
                FechaViaje = a.FechaViaje,
                DesvioKm = a.DesvioKm,
                TiempoExtraMin = a.TiempoExtraMin,
                Ganancia = a.Ganancia
            }).ToList());
        }

        // null si algún envío no existe. Evalúa cada envío con las mismas reglas que Aceptar y después la
        // selección completa: entre todos no pueden superar el lugar libre ni el desvío máximo del viaje
        // (el desvío de la selección es la suma de los desvíos, igual que en el Optimizador).
        private async Task<EvaluacionSeleccion?> EvaluarSeleccionAsync(Viaje viaje, List<int> envioIds, int usuarioId)
        {
            var hoy = Hoy();
            var seleccion = new EvaluacionSeleccion();

            foreach (var envioId in envioIds.Distinct())
            {
                var envio = await _envioRepositorio.ObtenerPorIdAsync(envioId);
                if (envio == null)
                {
                    return null;
                }

                var evaluacion = await EvaluarInternoAsync(viaje, envio, usuarioId, hoy, prefiltrar: false);
                seleccion.Items.Add(new ItemSeleccion(envio, evaluacion));

                if (seleccion.Motivo == null && evaluacion.Motivo != null)
                {
                    seleccion.Motivo = envioIds.Count == 1
                        ? evaluacion.Motivo
                        : $"{envio.OrigenDir} → {envio.DestinoDir}: {evaluacion.Motivo}";
                }
            }

            seleccion.Motivo ??= MotivoSeleccionSinLugar(viaje, seleccion.Items);
            return seleccion;
        }

        private static string? MotivoSeleccionSinLugar(Viaje viaje, List<ItemSeleccion> items)
        {
            var (paquetesLibres, kgLibres) = CapacidadRestante(viaje);

            if (items.Count > paquetesLibres)
            {
                return $"La selección tiene {items.Count} paquetes y el viaje tiene lugar para {Math.Max(paquetesLibres, 0)}.";
            }

            var pesoTotal = items.Sum(i => i.Envio.PesoKg);
            if (pesoTotal > kgLibres)
            {
                return $"La selección suma {pesoTotal:0.##} kg y el viaje tiene lugar para {Math.Max(kgLibres, 0):0.##} kg.";
            }

            var desvioTotal = items.Sum(i => i.Evaluacion.Ruta?.DesvioKm ?? 0);
            if (desvioTotal > viaje.DesvioMaxKm)
            {
                return $"La selección suma {desvioTotal:0.##} km de desvío y el máximo del viaje es {viaje.DesvioMaxKm:0.##} km.";
            }

            return null;
        }

        private static OportunidadDto ADtoSeleccion(ItemSeleccion item, Viaje viaje)
        {
            var desvio = item.Evaluacion.Ruta?.DesvioKm ?? 0;
            var (compatibilidad, puntuacion) = CalcularCompatibilidad(desvio, viaje.DesvioMaxKm);

            return new OportunidadDto
            {
                EnvioId = item.Envio.Id,
                Origen = item.Envio.OrigenDir,
                Destino = item.Envio.DestinoDir,
                Descripcion = item.Envio.Descripcion,
                PesoKg = item.Envio.PesoKg,
                DesvioKm = desvio,
                MontoPago = item.Envio.Monto,
                FechaViaje = item.Evaluacion.Fecha ?? default,
                Compatibilidad = compatibilidad,
                PuntuacionCompatibilidad = puntuacion
            };
        }

        private record ItemSeleccion(Envio Envio, EvaluacionOportunidad Evaluacion);

        private class EvaluacionSeleccion
        {
            public List<ItemSeleccion> Items { get; } = new List<ItemSeleccion>();
            public string? Motivo { get; set; }
        }

        // Expuesto para el Optimizador: evalúa si un envío ya cargado entra en el viaje dado.
        // Usa prefiltrar: false porque el Optimizador ya tiene la lista filtrada de candidatos.
        public async Task<EvaluacionOportunidad> EvaluarCompatibilidadAsync(Viaje viaje, Envio envio, int usuarioId, DateOnly hoy)
        {
            return await EvaluarInternoAsync(viaje, envio, usuarioId, hoy, prefiltrar: false);
        }

        private const string MotivoNoDisponible = "El envío ya no está disponible.";

        // Aplica las reglas en orden y devuelve el primer motivo por el que el envío no entra en el viaje
        // (null si entra), la fecha en la que se llevaría y la comparación de rutas.
        private async Task<EvaluacionOportunidad> EvaluarInternoAsync(Viaje viaje, Envio envio, int usuarioId, DateOnly hoy, bool prefiltrar)
        {
            var evaluacion = new EvaluacionOportunidad();

            if (!viaje.Activo)
            {
                return evaluacion.ConMotivo("El viaje está inactivo. Activalo para aceptar envíos.");
            }

            if (envio.UsuarioId == usuarioId)
            {
                return evaluacion.ConMotivo("No podés transportar un envío que publicaste vos.");
            }

            if (!EstaDisponible(envio))
            {
                return evaluacion.ConMotivo(MotivoNoDisponible);
            }

            if (envio.FechaLimite < hoy)
            {
                return evaluacion.ConMotivo("El envío ya venció.");
            }

            evaluacion.Fecha = FechasDelViaje(viaje, hoy, envio.FechaLimite).Cast<DateOnly?>().FirstOrDefault();
            var motivoCapacidad = MotivoSinLugar(viaje, envio.PesoKg);

            var recorrido = RecorridoHabitual(viaje);
            if (prefiltrar && DesvioEnLineaRectaKm(recorrido, envio) > viaje.DesvioMaxKm * MargenPrefiltro)
            {
                return evaluacion.ConMotivo("El desvío supera el máximo del viaje.");
            }

            // Antes de consultar rutas, en el listado alcanza con saber que no hay fecha o lugar.
            if (prefiltrar && (evaluacion.Fecha == null || motivoCapacidad != null))
            {
                return evaluacion.ConMotivo(motivoCapacidad ?? "Sin fecha disponible.");
            }

            evaluacion.Ruta = await CompararConEnvioAsync(recorrido, envio);

            if (evaluacion.Fecha == null)
            {
                return evaluacion.ConMotivo($"El viaje no sale ningún día antes de la fecha límite del envío ({envio.FechaLimite:dd/MM/yyyy}).");
            }

            if (motivoCapacidad != null)
            {
                return evaluacion.ConMotivo(motivoCapacidad);
            }

            if (evaluacion.Ruta == null)
            {
                return evaluacion.ConMotivo("No se pudo calcular la ruta con el envío. Probá de nuevo en unos minutos.");
            }

            if (evaluacion.Ruta.DesvioKm > viaje.DesvioMaxKm)
            {
                return evaluacion.ConMotivo($"El desvío ({evaluacion.Ruta.DesvioKm:0.##} km) supera el máximo del viaje ({viaje.DesvioMaxKm:0.##} km).");
            }

            return evaluacion;
        }

        private async Task<DetalleOportunidadRutaDto?> CompararConEnvioAsync(List<CoordenadaDto> recorrido, Envio envio)
        {
            try
            {
                return await _rutaServicio.CompararConEnvioAsync(
                    recorrido,
                    new CoordenadaDto(envio.OrigenLat, envio.OrigenLng),
                    new CoordenadaDto(envio.DestinoLat, envio.DestinoLng));
            }
            catch (Exception ex)
            {
                // Un envío sin ruta se descarta, pero se deja rastro para no confundirlo con "no hay oportunidades".
                _logger.LogWarning(ex, "No se pudo calcular la ruta del envío {EnvioId}.", envio.Id);
                return null;
            }
        }

        private async Task<Viaje?> ObtenerViajePropioAsync(int usuarioId, int viajeId)
        {
            var viaje = await _viajeRepositorio.ObtenerPorIdAsync(viajeId);
            return viaje != null && viaje.UsuarioId == usuarioId ? viaje : null;
        }

        private static bool EstaDisponible(Envio envio)
        {
            return envio.Estado == EstadoEnvio.BuscandoConductor && envio.Asignacion == null;
        }

        // Fechas entre hoy y la fecha límite del envío que caen en los días del viaje.
        private static IEnumerable<DateOnly> FechasDelViaje(Viaje viaje, DateOnly desde, DateOnly hasta)
        {
            var dias = viaje.Dias.Select(d => d.DiaSemana).ToHashSet();
            for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
            {
                if (dias.Contains(fecha.ADiaSemana()))
                {
                    yield return fecha;
                }
            }
        }

        // null si el envío entra en el lugar libre del viaje; si no, explica qué límite superaría.
        private static string? MotivoSinLugar(Viaje viaje, decimal pesoKg)
        {
            var (paquetesLibres, kgLibres) = CapacidadRestante(viaje);

            if (paquetesLibres < 1)
            {
                return $"El viaje ya tiene {viaje.MaxPaquetes} de {viaje.MaxPaquetes} paquetes aceptados.";
            }

            if (pesoKg > kgLibres)
            {
                var ocupadosKg = viaje.PesoMaxKg - kgLibres;
                return $"Con este envío el viaje llevaría {ocupadosKg + pesoKg:0.##} kg y el máximo es {viaje.PesoMaxKg:0.##} kg.";
            }

            return null;
        }

        // Lugar libre del viaje: el límite de paquetes y de peso es del viaje, no de cada salida. Ocupan lugar
        // los envíos aceptados que todavía no se entregaron; los entregados y los cancelados lo liberan.
        internal static (int Paquetes, decimal Kg) CapacidadRestante(Viaje viaje)
        {
            var ocupadas = viaje.Asignaciones
                .Where(a => a.Envio != null && OcupaLugar(a.Envio.Estado))
                .ToList();

            return (viaje.MaxPaquetes - ocupadas.Count, viaje.PesoMaxKg - ocupadas.Sum(a => a.Envio.PesoKg));
        }

        private static bool OcupaLugar(EstadoEnvio estado)
        {
            return estado == EstadoEnvio.ConductorAsignado
                || estado == EstadoEnvio.Retirado
                || estado == EstadoEnvio.EnCamino;
        }

        internal static (string Texto, decimal Puntuacion) CalcularCompatibilidad(decimal desvioKm, decimal desvioMaxKm)
        {
            var proporcion = desvioMaxKm > 0 ? Math.Clamp(desvioKm / desvioMaxKm, 0, 1) : 1;
            var texto = proporcion <= UmbralMuyCompatible ? "Muy compatible"
                : proporcion <= UmbralCompatible ? "Compatible"
                : "Mucho desvío";

            return (texto, Math.Round(1 - proporcion, 2));
        }

        // Los empates se resuelven siempre igual: a igual ganancia, menor desvío (y viceversa).
        private static List<OportunidadDto> Ordenar(List<OportunidadDto> oportunidades, string? ordenarPor)
        {
            return ordenarPor?.ToLowerInvariant() switch
            {
                "desvio" => oportunidades.OrderBy(o => o.DesvioKm).ThenByDescending(o => o.MontoPago).ThenBy(o => o.EnvioId).ToList(),
                "compatibilidad" => oportunidades.OrderByDescending(o => o.PuntuacionCompatibilidad).ThenByDescending(o => o.MontoPago).ThenBy(o => o.EnvioId).ToList(),
                _ => oportunidades.OrderByDescending(o => o.MontoPago).ThenBy(o => o.DesvioKm).ThenBy(o => o.EnvioId).ToList()
            };
        }

        private static decimal DesvioEnLineaRectaKm(List<CoordenadaDto> recorrido, Envio envio)
        {
            var conEnvio = InsercionRecorrido.Insertar(
                recorrido,
                new CoordenadaDto(envio.OrigenLat, envio.OrigenLng),
                new CoordenadaDto(envio.DestinoLat, envio.DestinoLng));

            return GeoUtil.LongitudKm(conEnvio) - GeoUtil.LongitudKm(recorrido);
        }

        internal static List<CoordenadaDto> RecorridoHabitual(Viaje viaje)
        {
            var recorrido = new List<CoordenadaDto> { new(viaje.OrigenLat, viaje.OrigenLng) };
            recorrido.AddRange(viaje.Paradas.OrderBy(p => p.Orden).Select(p => new CoordenadaDto(p.Lat, p.Lng)));
            recorrido.Add(new CoordenadaDto(viaje.DestinoLat, viaje.DestinoLng));
            return recorrido;
        }

        // "Hoy" en Argentina, independiente de la zona horaria del servidor (ver AGENTS 6.3).
        internal static DateOnly Hoy()
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.Add(_offsetArgentina));
        }
    }
}
