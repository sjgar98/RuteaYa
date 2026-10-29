using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Rutas;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class OportunidadesServicio : IOportunidadesServicio
    {
        private readonly IViajeRepositorio _viajeRepositorio;
        private readonly IEnvioRepositorio _envioRepositorio;
        private readonly IAsignacionRepositorio _asignacionRepositorio;
        private readonly IRutaServicio _rutaServicio;

        public OportunidadesServicio(
            IViajeRepositorio viajeRepositorio,
            IEnvioRepositorio envioRepositorio,
            IAsignacionRepositorio asignacionRepositorio,
            IRutaServicio rutaServicio)
        {
            _viajeRepositorio = viajeRepositorio;
            _envioRepositorio = envioRepositorio;
            _asignacionRepositorio = asignacionRepositorio;
            _rutaServicio = rutaServicio;
        }

        public async Task<List<OportunidadDto>> ObtenerOportunidadesAsync(int viajeId, string? ordenarPor = "ganancia")
        {
            // Obtener el viaje del usuario
            var viaje = await _viajeRepositorio.ObtenerPorIdAsync(viajeId);
            if (viaje == null)
                return new List<OportunidadDto>();

            // ETAPA 1: Filtrado pre-ruta (sin llamar Google/IRutaServicio)
            var enviosDisponibles = await _envioRepositorio.ListarDisponiblesAsync(
                DateOnly.FromDateTime(DateTime.Today),
                viaje.UsuarioId
            );

            // Calcular capacidad restante (paquetes y peso)
            var asignacionesViajeActual = viaje.Asignaciones ?? new List<Asignacion>();
            int paquetesUsados = asignacionesViajeActual.Count;
            decimal pesoUsado = asignacionesViajeActual.Sum(a => a.Envio?.PesoKg ?? 0);

            int capacidadRestantePaquetes = viaje.MaxPaquetes - paquetesUsados;
            decimal capacidadRestanteKg = viaje.PesoMaxKg - pesoUsado;

            // Filtrar: solo envíos que cumplen restricciones básicas
            var candidatos = new List<(Envio envio, int capacidadRestante, decimal pesoRestante)>();

            foreach (var envio in enviosDisponibles)
            {
                // Filtro 1: Peso total no puede exceder capacidad
                if (envio.PesoKg > capacidadRestanteKg)
                    continue;

                // Filtro 2: Si llegamos al máximo de paquetes, no se puede agregar más
                if (capacidadRestantePaquetes <= 0)
                    continue;

                // Filtro 3: Línea recta (Haversine) como pre-filtro
                decimal distanciaLineaRecta = CalcularDistanciaHaversine(
                    viaje.OrigenLat, viaje.OrigenLng,
                    envio.DestinoLat, envio.DestinoLng
                );

                // Si la línea recta ya supera el desvío máximo, descartar
                if (distanciaLineaRecta > viaje.DesvioMaxKm)
                    continue;

                candidatos.Add((envio, capacidadRestantePaquetes, capacidadRestanteKg));
            }

            // ETAPA 2: Cálculo exacto (llamar IRutaServicio solo para candidatos que pasaron)
            var oportunidades = new List<OportunidadDto>();

            foreach (var (envio, _, _) in candidatos)
            {
                try
                {
                    // Obtener detalles exactos de la ruta
                    var detalleRuta = await _rutaServicio.ObtenerDetalleOportunidadAsync(
                        viaje.OrigenDir,
                        viaje.DestinoDir,
                        envio.OrigenDir,
                        envio.DestinoDir
                    );

                    // Filtro 4: Desvío exacto no puede exceder límite del viaje
                    if (detalleRuta.DesvioKm > viaje.DesvioMaxKm)
                        continue;

                    // Calcular compatibilidad
                    var (compatibilidadTexto, score) = CalcularCompatibilidad(envio, detalleRuta);

                    // Recalcular capacidad restante en tiempo real
                    int paqRestantes = viaje.MaxPaquetes - (asignacionesViajeActual.Count);
                    decimal pesoRestante = viaje.PesoMaxKg - asignacionesViajeActual.Sum(a => a.Envio?.PesoKg ?? 0);

                    oportunidades.Add(new OportunidadDto
                    {
                        EnvioId = envio.Id,
                        Origen = envio.OrigenDir,
                        Destino = envio.DestinoDir,
                        PesoKg = envio.PesoKg,
                        DesvioKm = detalleRuta.DesvioKm,
                        MontoPago = envio.Monto,
                        Compatibilidad = compatibilidadTexto,
                        PuntuacionCompatibilidad = score,
                        CapacidadRestantePaquetes = paqRestantes,
                        CapacidadRestanteKg = pesoRestante
                    });
                }
                catch
                {
                    // Si falla el cálculo de ruta, descartar este envío
                    continue;
                }
            }

            // Ordenar según parámetro
            var resultado = ordenarPor switch
            {
                "desvio" => oportunidades.OrderBy(o => o.DesvioKm).ToList(),
                "compatibilidad" => oportunidades.OrderByDescending(o => o.PuntuacionCompatibilidad).ToList(),
                _ => oportunidades.OrderByDescending(o => o.MontoPago).ToList() // ganancia (default)
            };

            return resultado;
        }

        public async Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int viajeId, int envioId)
        {
            // Validar que el viaje pertenezca al usuario y exista
            var viaje = await _viajeRepositorio.ObtenerPorIdAsync(viajeId);
            if (viaje == null)
                return null;

            // Obtener el envío
            var envio = await _envioRepositorio.ObtenerPorIdAsync(envioId);
            if (envio == null || envio.Estado != EstadoEnvio.BuscandoConductor)
                return null;

            // Obtener detalles de la ruta
            var detalleRuta = await _rutaServicio.ObtenerDetalleOportunidadAsync(
                viaje.OrigenDir,
                viaje.DestinoDir,
                envio.OrigenDir,
                envio.DestinoDir
            );

            // Mapear envío a DTO
            var envioDto = new EnvioDto
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
            };

            return new DetalleOportunidadDto
            {
                EnvioId = envio.Id,
                ViajeId = viaje.Id,
                Envio = envioDto,
                RutaDetalle = detalleRuta
            };
        }

        public async Task<ResultadoOperacion> AceptarAsync(int viajeId, int envioId)
        {
            try
            {
                // Obtener viaje
                var viaje = await _viajeRepositorio.ObtenerPorIdAsync(viajeId);
                if (viaje == null)
                    return ResultadoOperacion.NoEncontrado;

                // Obtener envío
                var envio = await _envioRepositorio.ObtenerPorIdAsync(envioId);
                if (envio == null || envio.Estado != EstadoEnvio.BuscandoConductor)
                    return ResultadoOperacion.NoEncontrado;

                // Validar que no esté ya asignado
                if (viaje.Asignaciones.Any(a => a.EnvioId == envioId))
                    return ResultadoOperacion.Conflicto;

                // Validar capacidad de paquetes
                int paquetesActuales = viaje.Asignaciones.Count;
                if (paquetesActuales >= viaje.MaxPaquetes)
                    return ResultadoOperacion.Conflicto;

                // Validar capacidad de peso
                decimal pesoActual = viaje.Asignaciones.Sum(a => a.Envio?.PesoKg ?? 0);
                if (pesoActual + envio.PesoKg > viaje.PesoMaxKg)
                    return ResultadoOperacion.Conflicto;

                // Obtener detalles de ruta para validar desvío
                var detalleRuta = await _rutaServicio.ObtenerDetalleOportunidadAsync(
                    viaje.OrigenDir,
                    viaje.DestinoDir,
                    envio.OrigenDir,
                    envio.DestinoDir
                );

                // Validar desvío
                if (detalleRuta.DesvioKm > viaje.DesvioMaxKm)
                    return ResultadoOperacion.Conflicto;

                // Crear asignación
                var asignacion = new Asignacion
                {
                    ViajeId = viaje.Id,
                    EnvioId = envio.Id,
                    DesvioKm = detalleRuta.DesvioKm,
                    TiempoExtraMin = detalleRuta.TiempoAdicionalMinutos,
                    Ganancia = detalleRuta.MontoPago,
                    AsignadoEn = DateTime.Now,
                    Envio = envio
                };

                // Agregar asignación (como lista)
                await _asignacionRepositorio.AgregarAsync(new List<Asignacion> { asignacion });
                await _asignacionRepositorio.GuardarCambiosAsync();

                return ResultadoOperacion.Exito;
            }
            catch
            {
                return ResultadoOperacion.Conflicto;
            }
        }

        private (string compatibilidad, decimal score) CalcularCompatibilidad(Envio envio, DetalleOportunidadRutaDto detalleRuta)
        {
            // Cálculo de score basado en dinero/kg y dinero/km de desvío
            decimal dineroXkg = envio.Monto / envio.PesoKg;
            decimal dineroXkmDesvio = detalleRuta.DesvioKm > 0 ? envio.Monto / detalleRuta.DesvioKm : 0;

            // Ponderación: 40% dinero/kg, 60% dinero/km desvío
            decimal score = (dineroXkg * 0.4m) + (dineroXkmDesvio * 0.6m);

            // Clasificación por score
            string compatibilidad = score switch
            {
                >= 1000 => "Muy compatible",
                >= 500 => "Compatible",
                _ => "Mucho desvío"
            };

            return (compatibilidad, score);
        }

        private decimal CalcularDistanciaHaversine(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const int radioTierra = 6371; // Radio de la Tierra en km

            decimal dLat = (lat2 - lat1) * (decimal)Math.PI / 180;
            decimal dLon = (lon2 - lon1) * (decimal)Math.PI / 180;

            double dLatDouble = (double)dLat;
            double dLonDouble = (double)dLon;
            double lat1Double = (double)lat1 * Math.PI / 180;
            double lat2Double = (double)lat2 * Math.PI / 180;

            double a = Math.Pow(Math.Sin(dLatDouble / 2), 2)
                + Math.Cos(lat1Double)
                * Math.Cos(lat2Double)
                * Math.Pow(Math.Sin(dLonDouble / 2), 2);

            double c = 2 * Math.Asin(Math.Sqrt(a));
            return (decimal)(radioTierra * c);
        }
    }
}
