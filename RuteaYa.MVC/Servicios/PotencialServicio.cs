using System.Net.Http.Json;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class PotencialServicio : IPotencialServicio
    {
        private readonly HttpClient _httpClient;

        public PotencialServicio(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PotencialDto?> ObtenerPotencialAsync(string? mes, string? criterio)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(mes))
                queryParams.Add($"mes={Uri.EscapeDataString(mes)}");

            if (!string.IsNullOrEmpty(criterio))
                queryParams.Add($"criterio={Uri.EscapeDataString(criterio)}");

            var url = "api/potencial";
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            return await _httpClient.GetFromJsonAsync<PotencialDto>(url);
        }
    }
}