using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class AsignacionServicio : IAsignacionServicio
    {
        private readonly HttpClient _http;

        public AsignacionServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<ResumenSeleccionDto?> ObtenerResumenAsync(int viajeId, List<int> envioIds)
        {
            var envios = string.Concat(envioIds.Select(id => $"&envioIds={id}"));
            var respuesta = await _http.GetAsync($"api/asignaciones/resumen?viajeId={viajeId}{envios}");
            if (respuesta.StatusCode == HttpStatusCode.NotFound || respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<ResumenSeleccionDto>();
        }

        public async Task<ResultadoConfirmacion> ConfirmarAsync(int viajeId, List<int> envioIds)
        {
            var respuesta = await _http.PostAsJsonAsync("api/asignaciones", new ConfirmarSeleccionDto { ViajeId = viajeId, EnvioIds = envioIds });

            switch (respuesta.StatusCode)
            {
                case HttpStatusCode.NotFound:
                case HttpStatusCode.BadRequest:
                    return new ResultadoConfirmacion { NoEncontrado = true };
                case HttpStatusCode.Conflict:
                    var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                    return new ResultadoConfirmacion { Conflicto = problema?.Title ?? "No se pudo reservar la selección." };
            }

            respuesta.EnsureSuccessStatusCode();
            var asignaciones = await respuesta.Content.ReadFromJsonAsync<List<AsignacionDto>>();
            return new ResultadoConfirmacion { Cantidad = asignaciones?.Count ?? 0 };
        }
    }
}
