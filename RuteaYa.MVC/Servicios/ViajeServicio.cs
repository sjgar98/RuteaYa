using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;
using RuteaYa.DTOs.Rutas;

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
                return await LeerErroresAsync(respuesta);
            }

            respuesta.EnsureSuccessStatusCode();
            return new Dictionary<string, string[]>();
        }

        public async Task<ResultadoEdicionViaje> ActualizarAsync(int id, CrearViajeDto dto)
        {
            var respuesta = await _http.PutAsJsonAsync($"api/viajes/{id}", dto);

            switch (respuesta.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new ResultadoEdicionViaje { Errores = await LeerErroresAsync(respuesta) };
                case HttpStatusCode.NotFound:
                    return new ResultadoEdicionViaje { NoEncontrado = true };
                case HttpStatusCode.Conflict:
                    var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                    return new ResultadoEdicionViaje { Conflicto = problema?.Title ?? "No se pudo editar el viaje." };
            }

            respuesta.EnsureSuccessStatusCode();
            return new ResultadoEdicionViaje();
        }

        public Task<ResultadoCambioEstado> DesactivarAsync(int id)
        {
            return CambiarActivoAsync($"api/viajes/{id}/desactivar");
        }

        public Task<ResultadoCambioEstado> ActivarAsync(int id)
        {
            return CambiarActivoAsync($"api/viajes/{id}/activar");
        }

        public async Task<List<DireccionDto>> BuscarDireccionesAsync(string texto)
        {
            var url = $"api/viajes/direcciones?texto={Uri.EscapeDataString(texto)}";
            return await _http.GetFromJsonAsync<List<DireccionDto>>(url) ?? new List<DireccionDto>();
        }

        public async Task<RutaResultadoDto?> CalcularRutaAsync(List<CoordenadaDto> recorrido)
        {
            var respuesta = await _http.PostAsJsonAsync("api/rutas/calcular", new ConsultaRutaDto { Recorrido = recorrido });
            if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            return await respuesta.Content.ReadFromJsonAsync<RutaResultadoDto>();
        }

        private async Task<ResultadoCambioEstado> CambiarActivoAsync(string url)
        {
            var respuesta = await _http.PostAsync(url, null);

            switch (respuesta.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new ResultadoCambioEstado { NoEncontrado = true };
                case HttpStatusCode.Conflict:
                    var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                    return new ResultadoCambioEstado { Conflicto = problema?.Title ?? "No se pudo cambiar el estado del viaje." };
            }

            respuesta.EnsureSuccessStatusCode();
            return new ResultadoCambioEstado();
        }

        // Errores por campo del ValidationProblemDetails que devuelve [ApiController] en un 400.
        private static async Task<Dictionary<string, string[]>> LeerErroresAsync(HttpResponseMessage respuesta)
        {
            var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            return new Dictionary<string, string[]>(problema?.Errors ?? new Dictionary<string, string[]>());
        }
    }
}
