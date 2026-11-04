using Microsoft.EntityFrameworkCore;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class PotencialServicio : IPotencialServicio
    {
        private readonly IViajeRepositorio _viajeRepositorio;
        private readonly IEnvioRepositorio _envioRepositorio;

        public PotencialServicio(
            IViajeRepositorio viajeRepositorio,
            IEnvioRepositorio envioRepositorio)
        {
            _viajeRepositorio = viajeRepositorio;
            _envioRepositorio = envioRepositorio;
        }

        public async Task<PotencialMensualDto?> ObtenerPotencialAsync(int usuarioId, string mes, string? criterio = "ganancia")
        {
            if (!DateOnly.TryParse($"{mes}-01", out var fechaInicio))
            {
                fechaInicio = DateOnly.FromDateTime(DateTime.UtcNow);
            }
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var viajesActivos = await _viajeRepositorio.ListarPorUsuarioAsync(usuarioId);
            var criterioNorm = NormalizarCriterioTexto(criterio);

            if (viajesActivos == null || !viajesActivos.Any())
            {
                return new PotencialMensualDto
                {
                    Mes = mes,
                    Criterio = criterioNorm,
                    MejorRecorrido = new List<DetalleViajePotencialDto>()
                };
            }

            var enviosDisponibles = await _envioRepositorio.ListarDisponiblesRangoAsync(fechaInicio, fechaFin, usuarioId);

            var todosLosCandidatos = new List<(Viaje Viaje, Envio Envio, decimal Desvio, decimal Ganancia)>();
            var enviosAsignadosIds = new HashSet<int>();

            // 1. Recolectar todas las combinaciones posibles válidas
            foreach (var viaje in viajesActivos)
            {
                foreach (var envio in enviosDisponibles)
                {
                    if (enviosAsignadosIds.Contains(envio.Id)) continue;

                    decimal distanciaOrigenEnvio = CalcularDistanciaKmLocal(viaje.OrigenLat, viaje.OrigenLng, envio.OrigenLat, envio.OrigenLng);
                    decimal distanciaEnvioDestino = CalcularDistanciaKmLocal(envio.OrigenLat, envio.OrigenLng, envio.DestinoLat, envio.DestinoLng);
                    decimal distanciaDestinoFinal = CalcularDistanciaKmLocal(envio.DestinoLat, envio.DestinoLng, viaje.DestinoLat, viaje.DestinoLng);

                    decimal desvioTotal = distanciaOrigenEnvio + distanciaEnvioDestino + distanciaDestinoFinal - viaje.DistanciaKm;
                    if (desvioTotal < 0) desvioTotal = 0;

                    if (desvioTotal <= viaje.DesvioMaxKm && envio.PesoKg <= viaje.PesoMaxKg)
                    {
                        todosLosCandidatos.Add((viaje, envio, desvioTotal, envio.Monto));
                    }
                }
            }

            var detallesRecorrido = new List<DetalleViajePotencialDto>();
            var enviosProcesados = new HashSet<int>();

            // 2. Aplicar el filtro según el criterio seleccionado
            var candidatosFiltrados = criterioNorm switch
            {
                "desvio" => todosLosCandidatos
                    .Where(c => c.Desvio <= 2.0m) // Filtro estricto de desvío bajo
                    .OrderBy(c => c.Desvio)
                    .ThenByDescending(c => c.Ganancia)
                    .ToList(),

                "ganancia" => todosLosCandidatos
                    .OrderByDescending(c => c.Ganancia)
                    .ThenBy(c => c.Desvio)
                    .ToList(),

                "equilibrio" => todosLosCandidatos
                    .OrderByDescending(c => c.Ganancia / (c.Desvio + 1m))
                    .ToList(),

                _ => todosLosCandidatos // "todos"
                    .OrderByDescending(c => c.Envio.FechaLimite)
                    .ToList()
            };

            // Si para "desvio" o "equilibrio" el filtro estricto deja la lista vacía por falta de datos, 
            // tomamos al menos las mejores opciones disponibles para no romper la pantalla.
            if (!candidatosFiltrados.Any() && todosLosCandidatos.Any())
            {
                candidatosFiltrados = todosLosCandidatos;
            }

            // 3. Mapear a los detalles evitando duplicar envíos en el mismo set
            foreach (var c in candidatosFiltrados)
            {
                if (enviosProcesados.Contains(c.Envio.Id)) continue;
                enviosProcesados.Add(c.Envio.Id);

                detallesRecorrido.Add(new DetalleViajePotencialDto
                {
                    Fecha = c.Envio.FechaLimite,
                    OrigenDestino = $"{c.Viaje.OrigenDir} ➔ {c.Viaje.DestinoDir}",
                    PaquetesAsignados = 1,
                    PesoTotalKg = c.Envio.PesoKg,
                    DesvioTotalKm = Math.Round(c.Desvio, 2),
                    GananciaEstimada = c.Ganancia
                });
            }

            // Para el criterio "ganancia" o "equilibrio", si queremos acotarlo a 1 por viaje principal como antes, o mostrar todos los ordenados:
            if (criterioNorm == "ganancia" || criterioNorm == "equilibrio")
            {
                // Tomamos el mejor por cada viaje activo para que sea un plan óptimo real
                var unicosPorViaje = detallesRecorrido
                    .GroupBy(d => d.OrigenDestino)
                    .Select(g => g.First())
                    .ToList();
                detallesRecorrido = unicosPorViaje;
            }

            // 4. Recálculo total de métricas
            decimal gananciaTotal = detallesRecorrido.Sum(d => d.GananciaEstimada);
            int paquetesPosibles = detallesRecorrido.Count;
            var promedioViaje = viajesActivos.Count != 0 ? Math.Round(gananciaTotal / viajesActivos.Count, 2) : 0;

            return new PotencialMensualDto
            {
                Mes = mes,
                Criterio = criterioNorm,
                GananciaPotencial = gananciaTotal,
                PromedioPorViaje = promedioViaje,
                GananciaPorSemana = Math.Round(gananciaTotal / 4m, 2),
                PaquetesPosibles = paquetesPosibles,
                ViajesAnalizados = viajesActivos.Count,
                MejorRecorrido = detallesRecorrido
            };
        }

        private decimal CalcularDistanciaKmLocal(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var dLat = (double)(lat2 - lat1) * Math.PI / 180.0;
            var dLon = (double)(lon2 - lon1) * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos((double)lat1 * Math.PI / 180.0) * Math.Cos((double)lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distanciaLineaRecta = 6371.0 * c;

            return (decimal)(distanciaLineaRecta * 1.3);
        }

        private static string NormalizarCriterioTexto(string? criterio)
        {
            return criterio?.ToLowerInvariant() switch
            {
                "todos" => "todos",
                "desvio" => "desvio",
                "equilibrio" => "equilibrio",
                _ => "ganancia"
            };
        }
    }
}