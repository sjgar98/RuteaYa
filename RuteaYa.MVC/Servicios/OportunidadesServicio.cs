using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class OportunidadesServicio : IOportunidadesServicio
    {
        private readonly HttpClient _http;

        public OportunidadesServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<OportunidadesViajeDto?> ListarAsync(int viajeId, string ordenarPor)
        {
            var respuesta = await _http.GetAsync($"api/oportunidades/{viajeId}?ordenarPor={Uri.EscapeDataString(ordenarPor)}");
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<OportunidadesViajeDto>();
        }

        public async Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int viajeId, int envioId)
        {
            var respuesta = await _http.GetAsync($"api/oportunidades/{viajeId}/detalle?envioId={envioId}");
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<DetalleOportunidadDto>();
        }

        public async Task<ResultadoAceptarOportunidad> AceptarAsync(int viajeId, int envioId)
        {
            var respuesta = await _http.PostAsJsonAsync($"api/oportunidades/{viajeId}/aceptar", new AceptarOportunidadDto { EnvioId = envioId });

            switch (respuesta.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new ResultadoAceptarOportunidad { NoEncontrado = true };
                case HttpStatusCode.Conflict:
                    var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                    return new ResultadoAceptarOportunidad { Conflicto = problema?.Title ?? "No se pudo aceptar la oportunidad." };
            }

            respuesta.EnsureSuccessStatusCode();
            return new ResultadoAceptarOportunidad();
        }
    }
}
