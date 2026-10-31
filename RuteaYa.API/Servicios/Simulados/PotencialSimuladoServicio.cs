using Microsoft.EntityFrameworkCore;
using RuteaYa.DTOs;
using RuteaYa.Entidades;

namespace RuteaYa.API.Servicios.Simulados
{
    public interface IPotencialServicio
    {
        Task<PotencialDto> CalcularPotencialMensualAsync(int usuarioId, string? mes, string? criterio);
    }

    public class PotencialSimuladoServicio : IPotencialServicio
    {
        private readonly RuteaYaDbContext _context;

        public PotencialSimuladoServicio(RuteaYaDbContext context)
        {
            _context = context;
        }

        public async Task<PotencialDto> CalcularPotencialMensualAsync(int usuarioId, string? mesStr, string? criterio)
        {
            // 1. Determinar el mes a analizar (por defecto el mes actual)
            DateTime mesObjetivo = DateTime.Now;
            if (!string.IsNullOrEmpty(mesStr) && DateTime.TryParse(mesStr + "-01", out var parsedDate))
            {
                mesObjetivo = parsedDate;
            }

            string mesFormato = mesObjetivo.ToString("yyyy-MM");
            string criterioSeleccionado = string.IsNullOrEmpty(criterio) ? "ganancia" : criterio;

            // 2. Obtener viajes activos del usuario con sus días recurrentes
            var viajes = await _context.Viajes
                .Include(v => v.Dias)
                .Where(v => v.UsuarioId == usuarioId && v.Activo)
                .ToListAsync();

            // 3. Obtener envíos disponibles en el sistema para simular oportunidades
            var enviosDisponibles = await _context.Envios
                .Where(e => e.UsuarioId != usuarioId && e.Estado == DTOs.Enums.EstadoEnvio.BuscandoConductor)
                .ToListAsync();

            var detalleViajes = new List<PotencialDetalleViajeDto>();
            var enviosUsadosIds = new HashSet<int>();

            int diasEnMes = DateTime.DaysInMonth(mesObjetivo.Year, mesObjetivo.Month);
            var random = new Random(100); // Semilla fija para simulación consistente

            // 4. Recorrer los días del mes y calcular ocurrencias de viajes
            for (int dia = 1; dia <= diasEnMes; dia++)
            {
                var fechaActual = new DateTime(mesObjetivo.Year, mesObjetivo.Month, dia);
                // Convertir DayOfWeek de C# a tu enum DiaSemana (Lunes=1 ... Domingo=7)
                int diaSemanaInt = ((int)fechaActual.DayOfWeek == 0) ? 7 : (int)fechaActual.DayOfWeek;

                foreach (var viaje in viajes)
                {
                    // Verificar si el viaje ocurre este día de la semana
                    bool ocurreHoy = viaje.Dias.Any(d => (int)d.DiaSemana == diaSemanaInt);
                    if (!ocurreHoy) continue;

                    // Seleccionar envíos simulados disponibles que no hayan sido usados
                    var candidatos = enviosDisponibles.Where(e => !enviosUsadosIds.Contains(e.Id)).ToList();
                    if (!candidatos.Any()) continue;

                    // Simulación del optimizador: selecciona entre 1 y min(3, candidatos.Count) paquetes
                    int cantidadPaquetesTomar = Math.Min(candidatos.Count, random.Next(1, 4));
                    var seleccionados = candidatos.OrderBy(c => random.Next()).Take(cantidadPaquetesTomar).ToList();

                    decimal gananciaViaje = 0;
                    decimal pesoViaje = 0;
                    decimal desvioViaje = 0;
                    var nombresPaquetes = new List<string>();

                    foreach (var envio in seleccionados)
                    {
                        enviosUsadosIds.Add(envio.Id); // Evitar duplicar envío en otro viaje
                        gananciaViaje += envio.Monto * (criterioSeleccionado == "ganancia" ? 1.2m : 1.0m);
                        pesoViaje += envio.PesoKg;
                        desvioViaje += (decimal)(random.NextDouble() * 1.5 + 0.5);
                        nombresPaquetes.Add($"{envio.OrigenDir} → {envio.DestinoDir} (${envio.Monto})");
                    }

                    detalleViajes.Add(new PotencialDetalleViajeDto
                    {
                        ViajeId = viaje.Id,
                        ViajeDescripcion = $"{viaje.OrigenDir} → {viaje.DestinoDir}",
                        FechaOcurrencia = fechaActual,
                        CantidadPaquetes = seleccionados.Count,
                        PesoTotalKg = pesoViaje,
                        DesvioTotalKm = Math.Round(desvioViaje, 1),
                        GananciaEstimada = gananciaViaje,
                        PaquetesAsignados = nombresPaquetes
                    });
                }
            }

            // 5. Agregaciones finales
            int totalViajesAnalizados = detalleViajes.Count;
            int totalPaquetes = detalleViajes.Sum(d => d.CantidadPaquetes);
            decimal gananciaTotal = detalleViajes.Sum(d => d.GananciaEstimada);
            decimal promedioViaje = totalViajesAnalizados > 0 ? Math.Round(gananciaTotal / totalViajesAnalizados, 2) : 0;
            decimal gananciaSemana = Math.Round(gananciaTotal / 4.2m, 2); // Aprox 4.2 semanas por mes

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