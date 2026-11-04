using System.Globalization;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class PotencialServicio : IPotencialServicio
    {
        private static readonly TimeSpan _offsetArgentina = TimeSpan.FromHours(-3);

        private readonly IViajeRepositorio _viajeRepositorio;
        private readonly IOptimizadorServicio _optimizadorServicio;

        public PotencialServicio(IViajeRepositorio viajeRepositorio, IOptimizadorServicio optimizadorServicio)
        {
            _viajeRepositorio = viajeRepositorio;
            _optimizadorServicio = optimizadorServicio;
        }

        public Task<PotencialMensualDto?> ObtenerPotencialAsync(int usuarioId, string mes, string? criterio = "ganancia")
        {
            return ObtenerPotencialAsync(usuarioId, mes, criterio, DateOnly.FromDateTime(DateTime.UtcNow.Add(_offsetArgentina)));
        }

        public async Task<PotencialMensualDto?> ObtenerPotencialAsync(int usuarioId, string mes, string? criterio, DateOnly hoy)
        {
            if (!IntentarLeerMes(mes, out var inicioMes))
            {
                return null;
            }

            var criterioNorm = NormalizarCriterio(criterio);
            var resultado = new PotencialMensualDto
            {
                Mes = inicioMes.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                Criterio = criterioNorm
            };

            var desde = inicioMes > hoy ? inicioMes : hoy;
            var hasta = inicioMes.AddMonths(1).AddDays(-1);
            if (desde > hasta)
            {
                return resultado;
            }

            var viajes = await _viajeRepositorio.ListarPorUsuarioAsync(usuarioId, true);
            var enviosUsados = new HashSet<int>();

            foreach (var viaje in viajes.OrderBy(v => v.Id))
            {
                var salidas = ContarSalidas(viaje, desde, hasta);
                if (salidas == 0)
                {
                    continue;
                }

                resultado.ViajesAnalizados++;

                var optimizacion = await _optimizadorServicio.OptimizarViajeAsync(viaje, usuarioId, criterioNorm, desde, hasta, enviosUsados);
                var optima = optimizacion.Optima;
                if (optima.Envios.Count == 0)
                {
                    continue;
                }

                var origenDestino = $"{viaje.OrigenDir} → {viaje.DestinoDir}";
                enviosUsados.UnionWith(optima.Envios.Select(e => e.EnvioId));

                resultado.MejorRecorrido.Add(new DetalleViajePotencialDto
                {
                    ViajeId = viaje.Id,
                    OrigenDestino = origenDestino,
                    Fecha = optima.Envios.Min(e => e.FechaViaje),
                    SalidasEnPeriodo = salidas,
                    PaquetesAsignados = optima.CantidadPaquetes,
                    PesoTotalKg = optima.PesoTotalKg,
                    DesvioTotalKm = optima.DesvioTotalKm,
                    GananciaEstimada = optima.GananciaTotal
                });

                resultado.DetalleAsignaciones.AddRange(optima.Envios.Select(e => new DetalleAsignacionPotencialDto
                {
                    EnvioId = e.EnvioId,
                    Descripcion = e.Descripcion,
                    OrigenDestinoEnvio = $"{e.Origen} → {e.Destino}",
                    Monto = e.MontoPago,
                    FechaViaje = e.FechaViaje,
                    ViajeAsociado = origenDestino
                }));
            }

            resultado.PaquetesPosibles = resultado.MejorRecorrido.Sum(r => r.PaquetesAsignados);
            resultado.GananciaPotencial = resultado.MejorRecorrido.Sum(r => r.GananciaEstimada);
            resultado.PromedioPorViaje = resultado.ViajesAnalizados == 0
                ? 0
                : Math.Round(resultado.GananciaPotencial / resultado.ViajesAnalizados, 2);

            var semanas = (hasta.DayNumber - desde.DayNumber + 1) / 7m;
            resultado.GananciaPorSemana = Math.Round(resultado.GananciaPotencial / semanas, 2);
            resultado.DetalleAsignaciones = resultado.DetalleAsignaciones.OrderBy(d => d.FechaViaje).ToList();

            return resultado;
        }

        public static bool IntentarLeerMes(string? mes, out DateOnly inicioMes)
        {
            return DateOnly.TryParseExact($"{mes}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out inicioMes);
        }

        private static int ContarSalidas(Viaje viaje, DateOnly desde, DateOnly hasta)
        {
            var dias = viaje.Dias.Select(d => d.DiaSemana).ToHashSet();
            var salidas = 0;
            for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
            {
                if (dias.Contains(fecha.ADiaSemana()))
                {
                    salidas++;
                }
            }

            return salidas;
        }

        private static string NormalizarCriterio(string? criterio)
        {
            return criterio?.ToLowerInvariant() switch
            {
                "desvio" => "desvio",
                "equilibrio" => "equilibrio",
                _ => "ganancia"
            };
        }
    }
}
