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
    // capacidad, peso y desvío) y con qué datos. Lo usan Oportunidades, Detalle y Aceptar.
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

            if (!viaje.Activo)
            {
                return resultado;
            }

            var hoy = Hoy();
            var envios = await _envioRepositorio.ListarDisponiblesAsync(hoy, usuarioId);
            var oportunidades = new List<OportunidadDto>();

            foreach (var envio in envios)
            {
                var evaluacion = await EvaluarAsync(viaje, envio, usuarioId, hoy, prefiltrar: true);
                if (evaluacion.Motivo != null)
                {
                    continue;
                }

                var fecha = evaluacion.Fecha!.Value;
                var (paquetesRestantes, kgRestantes) = CapacidadRestante(viaje, fecha);
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
                    CapacidadRestantePaquetes = paquetesRestantes,
                    CapacidadRestanteKg = kgRestantes
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
            var evaluacion = await EvaluarAsync(viaje, envio, usuarioId, hoy, prefiltrar: false);

            // Sin fecha con lugar, la capacidad se muestra para la próxima salida del viaje (si existe).
            var fechaReferencia = evaluacion.Fecha ?? FechasDelViaje(viaje, hoy, envio.FechaLimite).Cast<DateOnly?>().FirstOrDefault();
            var (paquetesRestantes, kgRestantes) = fechaReferencia.HasValue
                ? CapacidadRestante(viaje, fechaReferencia.Value)
                : (viaje.MaxPaquetes, viaje.PesoMaxKg);

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
            var viaje = await ObtenerViajePropioAsync(usuarioId, viajeId);
            if (viaje == null)
            {
                return ResultadoAsignacion.NoEncontrado();
            }

            var envio = await _envioRepositorio.ObtenerPorIdAsync(envioId);
            if (envio == null)
            {
                return ResultadoAsignacion.NoEncontrado();
            }

            // Se revalida todo: entre que el conductor vio la oportunidad y la aceptó pudo cambiar algo.
            var evaluacion = await EvaluarAsync(viaje, envio, usuarioId, Hoy(), prefiltrar: false);
            if (evaluacion.Motivo != null)
            {
                return ResultadoAsignacion.Conflicto(evaluacion.Motivo);
            }

            if (!EnvioEstadoMaquina.Transicionar(envio, EstadoEnvio.ConductorAsignado))
            {
                return ResultadoAsignacion.Conflicto(MotivoNoDisponible);
            }

            var asignacion = new Asignacion
            {
                ViajeId = viaje.Id,
                EnvioId = envio.Id,
                FechaViaje = evaluacion.Fecha!.Value,
                DesvioKm = evaluacion.Ruta!.DesvioKm,
                TiempoExtraMin = evaluacion.Ruta.TiempoAdicionalMinutos,
                Ganancia = envio.Monto,
                AsignadoEn = DateTime.UtcNow,
                Envio = envio
            };

            try
            {
                // Un solo SaveChanges guarda la asignación y el cambio de estado del envío (con su historial).
                await _asignacionRepositorio.AgregarAsync(new List<Asignacion> { asignacion });
            }
            catch (DbUpdateException ex)
            {
                // El índice único sobre EnvioId: otro conductor lo tomó al mismo tiempo.
                _logger.LogInformation(ex, "El envío {EnvioId} ya fue asignado por otro conductor.", envio.Id);
                return ResultadoAsignacion.Conflicto("Otro conductor acaba de tomar este envío.");
            }

            return ResultadoAsignacion.Exito(new AsignacionDto
            {
                Id = asignacion.Id,
                EnvioId = asignacion.EnvioId,
                ViajeId = asignacion.ViajeId,
                FechaViaje = asignacion.FechaViaje,
                DesvioKm = asignacion.DesvioKm,
                TiempoExtraMin = asignacion.TiempoExtraMin,
                Ganancia = asignacion.Ganancia
            });
        }

        private const string MotivoNoDisponible = "El envío ya no está disponible.";

        // Aplica las reglas en orden y devuelve el primer motivo por el que el envío no entra en el viaje
        // (null si entra), la fecha en la que se llevaría y la comparación de rutas.
        private async Task<Evaluacion> EvaluarAsync(Viaje viaje, Envio envio, int usuarioId, DateOnly hoy, bool prefiltrar)
        {
            var evaluacion = new Evaluacion();

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

            var fechas = FechasDelViaje(viaje, hoy, envio.FechaLimite).ToList();
            evaluacion.Fecha = fechas.Cast<DateOnly?>().FirstOrDefault(f => Entra(viaje, f!.Value, envio.PesoKg));

            var recorrido = RecorridoHabitual(viaje);
            if (prefiltrar && DesvioEnLineaRectaKm(recorrido, envio) > viaje.DesvioMaxKm * MargenPrefiltro)
            {
                return evaluacion.ConMotivo("El desvío supera el máximo del viaje.");
            }

            // Antes de consultar rutas, en el listado alcanza con saber que no hay fecha.
            if (prefiltrar && evaluacion.Fecha == null)
            {
                return evaluacion.ConMotivo("Sin fecha disponible.");
            }

            evaluacion.Ruta = await CompararConEnvioAsync(recorrido, envio);

            if (fechas.Count == 0)
            {
                return evaluacion.ConMotivo($"El viaje no sale ningún día antes de la fecha límite del envío ({envio.FechaLimite:dd/MM/yyyy}).");
            }

            if (evaluacion.Fecha == null)
            {
                return evaluacion.ConMotivo($"El viaje no tiene lugar (paquetes o peso) en ninguna salida hasta el {envio.FechaLimite:dd/MM/yyyy}.");
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

        private static bool Entra(Viaje viaje, DateOnly fecha, decimal pesoKg)
        {
            var (paquetesRestantes, kgRestantes) = CapacidadRestante(viaje, fecha);
            return paquetesRestantes >= 1 && kgRestantes >= pesoKg;
        }

        // La capacidad es por salida: solo cuentan las asignaciones de esa fecha cuyo envío no se canceló.
        private static (int Paquetes, decimal Kg) CapacidadRestante(Viaje viaje, DateOnly fecha)
        {
            var ocupadas = viaje.Asignaciones
                .Where(a => a.FechaViaje == fecha && a.Envio?.Estado != EstadoEnvio.Cancelado)
                .ToList();

            return (viaje.MaxPaquetes - ocupadas.Count, viaje.PesoMaxKg - ocupadas.Sum(a => a.Envio?.PesoKg ?? 0));
        }

        private static (string Texto, decimal Puntuacion) CalcularCompatibilidad(decimal desvioKm, decimal desvioMaxKm)
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

        private static List<CoordenadaDto> RecorridoHabitual(Viaje viaje)
        {
            var recorrido = new List<CoordenadaDto> { new(viaje.OrigenLat, viaje.OrigenLng) };
            recorrido.AddRange(viaje.Paradas.OrderBy(p => p.Orden).Select(p => new CoordenadaDto(p.Lat, p.Lng)));
            recorrido.Add(new CoordenadaDto(viaje.DestinoLat, viaje.DestinoLng));
            return recorrido;
        }

        // "Hoy" en Argentina, independiente de la zona horaria del servidor (ver AGENTS 6.3).
        private static DateOnly Hoy()
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.Add(_offsetArgentina));
        }

        private class Evaluacion
        {
            public string? Motivo { get; set; }
            public DateOnly? Fecha { get; set; }
            public DetalleOportunidadRutaDto? Ruta { get; set; }

            public Evaluacion ConMotivo(string motivo)
            {
                Motivo = motivo;
                return this;
            }
        }
    }
}
