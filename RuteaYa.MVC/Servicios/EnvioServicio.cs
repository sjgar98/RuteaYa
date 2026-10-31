using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class EnvioServicio : IEnvioServicio
    {
        private readonly HttpClient _http;

        public EnvioServicio(HttpClient http)
        {
            _http = http;
        }

        // El binder de la API acepta el nombre del enum en la query string (p. ej. "BuscandoConductor").
        public async Task<List<EnvioDto>> ListarAsync(EstadoEnvio? estado)
        {
            var url = estado.HasValue ? $"api/envios?estado={estado.Value}" : "api/envios";
            return await _http.GetFromJsonAsync<List<EnvioDto>>(url) ?? new List<EnvioDto>();
        }

        public async Task<EnvioDetalleDto?> ObtenerDetalleAsync(int id)
        {
            var respuesta = await _http.GetAsync($"api/envios/{id}");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<EnvioDetalleDto>();
        }

        public async Task<ResultadoCancelacionEnvio> CancelarAsync(int id)
        {
            var respuesta = await _http.PostAsync($"api/envios/{id}/cancelar", null);

            switch (respuesta.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new ResultadoCancelacionEnvio { NoEncontrado = true };
                case HttpStatusCode.Conflict:
                    var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                    return new ResultadoCancelacionEnvio { Conflicto = problema?.Title ?? "No se pudo cancelar el envío." };
            }

            respuesta.EnsureSuccessStatusCode();
            return new ResultadoCancelacionEnvio();
        }

        public async Task<Dictionary<string, string[]>> CrearAsync(CrearEnvioDto dto)
        {
            var respuesta = await _http.PostAsJsonAsync("api/envios", dto);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                return new Dictionary<string, string[]>(problema?.Errors ?? new Dictionary<string, string[]>());
            }

            respuesta.EnsureSuccessStatusCode();
            return new Dictionary<string, string[]>();
        }
    }
}