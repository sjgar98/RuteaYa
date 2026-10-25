using System.Net.Http.Json;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class ViajeServicio : IViajeServicio
    {
        private readonly HttpClient _http;

        public ViajeServicio(HttpClient http)
        {
            _http = http;
        }

        // La API toma el usuario del token, por eso no se envía el id.
        public async Task<List<ViajeDto>> ListarAsync(bool? activo)
        {
            var url = activo.HasValue ? $"api/viajes?activo={activo.Value.ToString().ToLower()}" : "api/viajes";
            return await _http.GetFromJsonAsync<List<ViajeDto>>(url) ?? new List<ViajeDto>();
        }
    }
}
