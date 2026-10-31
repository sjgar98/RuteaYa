using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class TransporteServicio : ITransporteServicio
    {
        private readonly HttpClient _http;

        public TransporteServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TransporteDto>> ListarAsync()
        {
            return await _http.GetFromJsonAsync<List<TransporteDto>>("api/transportes") ?? new List<TransporteDto>();
        }

        public Task<ResultadoCambioEstado> MarcarRetiradoAsync(int asignacionId)
        {
            return CambiarEstadoAsync($"api/transportes/{asignacionId}/retirado");
        }

        public Task<ResultadoCambioEstado> MarcarEnCaminoAsync(int asignacionId)
        {
            return CambiarEstadoAsync($"api/transportes/{asignacionId}/en-camino");
        }

        public Task<ResultadoCambioEstado> MarcarEntregadoAsync(int asignacionId)
        {
            return CambiarEstadoAsync($"api/transportes/{asignacionId}/entregado");
        }

        private async Task<ResultadoCambioEstado> CambiarEstadoAsync(string url)
        {
            var respuesta = await _http.PostAsync(url, null);

            switch (respuesta.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new ResultadoCambioEstado { NoEncontrado = true };
                case HttpStatusCode.Conflict:
                    var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                    return new ResultadoCambioEstado { Conflicto = problema?.Title ?? "No se pudo actualizar el estado." };
            }

            respuesta.EnsureSuccessStatusCode();
            return new ResultadoCambioEstado();
        }
    }
}