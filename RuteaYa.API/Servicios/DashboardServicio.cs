using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.API.Servicios
{
    // no consulta repositorios directo
    public class DashboardServicio : IDashboardServicio
    {
        private const int DiasProximasSalidas = 7;
        private const int MaximoEnLista = 5;
        private static readonly TimeSpan _offsetArgentina = TimeSpan.FromHours(-3);

        private static readonly EstadoEnvio[] _estadosPendientes =
        {
            EstadoEnvio.BuscandoConductor,
            EstadoEnvio.ConductorAsignado,
            EstadoEnvio.Retirado,
            EstadoEnvio.EnCamino
        };

        private readonly IViajeServicio _viajeServicio;
        private readonly IEnvioServicio _envioServicio;
        private readonly IOportunidadesServicio _oportunidadesServicio;
        private readonly IOptimizadorServicio _optimizadorServicio;

        public DashboardServicio(
            IViajeServicio viajeServicio,
            IEnvioServicio envioServicio,
            IOportunidadesServicio oportunidadesServicio,
            IOptimizadorServicio optimizadorServicio)
        {
            _viajeServicio = viajeServicio;
            _envioServicio = envioServicio;
            _oportunidadesServicio = oportunidadesServicio;
            _optimizadorServicio = optimizadorServicio;
        }

        public async Task<DashboardDto> ObtenerAsync(int usuarioId)
        {
            var ahora = DateTime.UtcNow.Add(_offsetArgentina);
            var viajesActivos = await _viajeServicio.ListarPorUsuarioAsync(usuarioId, true);

            var salidas = CalcularProximasSalidas(viajesActivos, ahora, DiasProximasSalidas);

            var enviosPendientes = (await _envioServicio.ListarPorUsuarioAsync(usuarioId, null))
                .Where(e => _estadosPendientes.Contains(e.Estado))
                .OrderBy(e => e.FechaLimite)
                .ToList();

            var dashboard = new DashboardDto
            {
                ViajesActivos = viajesActivos.Count,
                CantidadProximasSalidas = salidas.Count,
                ProximasSalidas = salidas.Take(MaximoEnLista).ToList(),
                CantidadEnviosPendientes = enviosPendientes.Count,
                EnviosPendientes = enviosPendientes.Take(MaximoEnLista).ToList()
            };

            // Un mismo envío puede entrar en varios viajes: se cuenta una sola vez.
            var enviosCompatibles = new HashSet<int>();
            foreach (var viaje in viajesActivos)
            {
                var oportunidades = await _oportunidadesServicio.ListarAsync(usuarioId, viaje.Id);
                enviosCompatibles.UnionWith(oportunidades?.Oportunidades.Select(o => o.EnvioId) ?? Enumerable.Empty<int>());
            }
            dashboard.CantidadOportunidades = enviosCompatibles.Count;

            // TODO Potencial: cuando exista el servicio de Potencial mensual (Pantalla 14), usar su total.
            // Mientras tanto se toma la combinación óptima de cada viaje, sin repetir envíos entre viajes
            // (quitar envíos de una combinación válida la deja válida).
            var hoy = DateOnly.FromDateTime(ahora);
            var enviosContados = new HashSet<int>();
            foreach (var viaje in viajesActivos)
            {
                var optimizacion = await _optimizadorServicio.OptimizarAsync(usuarioId, viaje.Id, "ganancia");
                var enviosDelMes = (optimizacion?.Optima.Envios ?? new List<OportunidadDto>())
                    .Where(e => e.FechaViaje.Year == hoy.Year && e.FechaViaje.Month == hoy.Month)
                    .Where(e => enviosContados.Add(e.EnvioId));

                foreach (var envio in enviosDelMes)
                {
                    dashboard.PotencialMes += envio.MontoPago;
                    dashboard.PaquetesPotencialMes++;
                }
            }

            return dashboard;
        }

        // Fechas de los próximos días en que sale cada viaje, en orden cronológico.
        // Hoy cuenta solo si la hora de salida todavía no pasó.
        public static List<ProximaSalidaDto> CalcularProximasSalidas(List<ViajeDto> viajes, DateTime ahora, int dias)
        {
            var hoy = DateOnly.FromDateTime(ahora);
            var horaActual = TimeOnly.FromDateTime(ahora);
            var salidas = new List<ProximaSalidaDto>();

            for (var i = 0; i < dias; i++)
            {
                var fecha = hoy.AddDays(i);
                foreach (var viaje in viajes.Where(v => v.Dias.Contains(fecha.ADiaSemana())))
                {
                    if (i == 0 && viaje.HoraSalida <= horaActual)
                    {
                        continue;
                    }

                    salidas.Add(new ProximaSalidaDto
                    {
                        ViajeId = viaje.Id,
                        OrigenDir = viaje.OrigenDir,
                        DestinoDir = viaje.DestinoDir,
                        Fecha = fecha,
                        HoraSalida = viaje.HoraSalida,
                        HoraLlegada = viaje.HoraLlegada
                    });
                }
            }

            return salidas.OrderBy(s => s.Fecha).ThenBy(s => s.HoraSalida).ToList();
        }
    }
}
