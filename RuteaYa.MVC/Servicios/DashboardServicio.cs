using System.Net.Http.Json;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class DashboardServicio : IDashboardServicio
    {
        private readonly HttpClient _http;

        public DashboardServicio(HttpClient http)
        {
            _http = http;
        }

        // La API toma el usuario del token.
        public async Task<DashboardDto> ObtenerAsync()
        {
            return await _http.GetFromJsonAsync<DashboardDto>("api/dashboard") ?? new DashboardDto();
        }
    }
}
