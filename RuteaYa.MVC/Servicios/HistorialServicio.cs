using System.Net.Http.Json;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class HistorialServicio : IHistorialServicio
    {
        readonly HttpClient _httpClient;

        public HistorialServicio(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HistorialDto?> ObtenerHistorialAsync(DateTime? desde, DateTime? hasta, string? orden, string? estado)
        {
            var url = "api/historial?";
            if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
            if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
            if (!string.IsNullOrEmpty(orden)) url += $"orden={orden}&";
            if (!string.IsNullOrEmpty(estado)) url += $"estado={estado}&";

            return await _httpClient.GetFromJsonAsync<HistorialDto>(url);
        }
    }
}