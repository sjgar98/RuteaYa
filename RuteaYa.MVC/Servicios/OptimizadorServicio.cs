using System.Net;
using System.Net.Http.Json;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class OptimizadorServicio : IOptimizadorServicio
    {
        private readonly HttpClient _http;

        public OptimizadorServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<OptimizacionResultadoDto?> OptimizarAsync(int viajeId, string criterio)
        {
            var respuesta = await _http.GetAsync($"api/optimizacion/{viajeId}?criterio={Uri.EscapeDataString(criterio)}");
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<OptimizacionResultadoDto>();
        }
    }
}

