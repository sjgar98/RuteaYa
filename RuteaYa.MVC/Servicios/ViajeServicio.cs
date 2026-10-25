using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<ViajeDetalleDto?> ObtenerDetalleAsync(int id)
        {
            var respuesta = await _http.GetAsync($"api/viajes/{id}");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<ViajeDetalleDto>();
        }

        public async Task<Dictionary<string, string[]>> CrearAsync(CrearViajeDto dto)
        {
            var respuesta = await _http.PostAsJsonAsync("api/viajes", dto);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                return new Dictionary<string, string[]>(problema?.Errors ?? new Dictionary<string, string[]>());
            }

            respuesta.EnsureSuccessStatusCode();
            return new Dictionary<string, string[]>();
        }

        public async Task<List<DireccionDto>> BuscarDireccionesAsync(string texto)
        {
            var url = $"api/viajes/direcciones?texto={Uri.EscapeDataString(texto)}";
            return await _http.GetFromJsonAsync<List<DireccionDto>>(url) ?? new List<DireccionDto>();
        }
    }
}
