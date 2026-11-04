using System.Net;
using System.Net.Http.Json;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class HistorialServicio : IHistorialServicio
    {
        private readonly HttpClient _http;

        public HistorialServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<HistorialDto?> ObtenerHistorialAsync(DateOnly? desde, DateOnly? hasta, string? orden, string? estado)
        {
            var parametros = new List<string>();
            if (desde.HasValue)
            {
                parametros.Add($"desde={desde.Value:yyyy-MM-dd}");
            }

            if (hasta.HasValue)
            {
                parametros.Add($"hasta={hasta.Value:yyyy-MM-dd}");
            }

            if (!string.IsNullOrEmpty(orden))
            {
                parametros.Add($"orden={Uri.EscapeDataString(orden)}");
            }

            if (!string.IsNullOrEmpty(estado))
            {
                parametros.Add($"estado={Uri.EscapeDataString(estado)}");
            }

            var respuesta = await _http.GetAsync($"api/historial?{string.Join("&", parametros)}");
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<HistorialDto>();
        }
    }
}
