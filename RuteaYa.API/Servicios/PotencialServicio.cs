using Microsoft.EntityFrameworkCore;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.Entidades;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class PotencialServicio : IPotencialServicio
    {
        private readonly RuteaYaDbContext _context;
        private readonly IOptimizadorServicio _optimizadorServicio;

        public PotencialServicio(
            RuteaYaDbContext context,
            IOptimizadorServicio optimizadorServicio)
        {
            _context = context;
            _optimizadorServicio = optimizadorServicio;
        }

        public async Task<PotencialDto> CalcularPotencialMensualAsync(int usuarioId, string? mesStr, string? criterio)
        {
            DateTime mesObjetivo = DateTime.Now;
            if (!string.IsNullOrEmpty(mesStr) && DateTime.TryParse(mesStr + "-01", out var parsedDate))
            {
                mesObjetivo = parsedDate;
            }

            string mesFormato = mesObjetivo.ToString("yyyy-MM");
            string criterioSeleccionado = string.IsNullOrEmpty(criterio) ? "ganancia" : criterio;

            // 1. Obtener viajes activos del usuario con sus días recurrentes
            var viajes = await _context.Viajes
                .Include(v => v.Dias)
                .Where(v => v.UsuarioId == usuarioId && v.Activo)
                .ToListAsync();

            var detalleViajes = new List<PotencialDetalleViajeDto>();
            var enviosUsadosIds = new HashSet<int>();

            int diasEnMes = DateTime.DaysInMonth(mesObjetivo.Year, mesObjetivo.Month);
            DateTime hoy = DateTime.Today;

            // 2. Recorrer cada día del mes
            for (int dia = 1; dia <= diasEnMes; dia++)
            {
                var fechaActual = new DateTime(mesObjetivo.Year, mesObjetivo.Month, dia);

                // Si es el mes actual, omitir días pasados
                if (mesObjetivo.Year == hoy.Year && mesObjetivo.Month == hoy.Month && fechaActual.Date < hoy)
                    continue;

                int diaSemanaInt = ((int)fechaActual.DayOfWeek == 0) ? 7 : (int)fechaActual.DayOfWeek;

                foreach (var viaje in viajes)
                {
                    bool ocurreHoy = viaje.Dias.Any(d => (int)d.DiaSemana == diaSemanaInt);
                    if (!ocurreHoy) continue;

                    // 3. Ejecutar el optimizador oficial para este viaje y criterio
                    var resultadoOptimizacion = await _optimizadorServicio.OptimizarAsync(usuarioId, viaje.Id, criterioSeleccionado);

                    if (resultadoOptimizacion?.Optima?.Envios != null && resultadoOptimizacion.Optima.Envios.Any())
                    {
                        // Filtrar los envíos óptimos que NO hayan sido utilizados ya en otro viaje de este mes
                        var enviosValidos = resultadoOptimizacion.Optima.Envios
                            .Where(e => !enviosUsadosIds.Contains(e.EnvioId))
                            .ToList();

                        if (!enviosValidos.Any()) continue;

                        decimal gananciaViaje = 0;
                        decimal pesoViaje = 0;
                        decimal desvioViaje = 0;
                        var nombresPaquetes = new List<string>();

                        foreach (var item in enviosValidos)
                        {
                            enviosUsadosIds.Add(item.EnvioId); // Marcar envío como usado en el mes
                            gananciaViaje += item.MontoPago;
                            pesoViaje += item.PesoKg;
                            desvioViaje += item.DesvioKm;
                            nombresPaquetes.Add($"{item.Descripcion} (${item.MontoPago})");
                        }

                        detalleViajes.Add(new PotencialDetalleViajeDto
                        {
                            ViajeId = viaje.Id,
                            ViajeDescripcion = $"{viaje.OrigenDir} → {viaje.DestinoDir}",
                            FechaOcurrencia = fechaActual,
                            CantidadPaquetes = enviosValidos.Count,
                            PesoTotalKg = pesoViaje,
                            DesvioTotalKm = Math.Round(desvioViaje, 1),
                            GananciaEstimada = gananciaViaje,
                            PaquetesAsignados = nombresPaquetes
                        });
                    }
                }
            }

            // 4. Calcular métricas agregadas del potencial mensual
            int totalViajesAnalizados = detalleViajes.Count;
            int totalPaquetes = detalleViajes.Sum(d => d.CantidadPaquetes);
            decimal gananciaTotal = detalleViajes.Sum(d => d.GananciaEstimada);
            decimal promedioViaje = totalViajesAnalizados > 0 ? Math.Round(gananciaTotal / totalViajesAnalizados, 2) : 0;
            decimal gananciaSemana = Math.Round(gananciaTotal / 4.2m, 2);

            string mejorRecorridoStr = detalleViajes.Any()
                ? detalleViajes.OrderByDescending(d => d.GananciaEstimada).First().ViajeDescripcion
                : "Sin datos suficientes";

            return new PotencialDto
            {
                Mes = mesFormato,
                Criterio = criterioSeleccionado,
                ViajesAnalizados = totalViajesAnalizados,
                PaquetesPosibles = totalPaquetes,
                GananciaPotencial = gananciaTotal,
                PromedioPorViaje = promedioViaje,
                GananciaPorSemana = gananciaSemana,
                MejorRecorrido = mejorRecorridoStr,
                DetalleViajes = detalleViajes.OrderBy(d => d.FechaOcurrencia).ToList()
            };
        }
    }
}