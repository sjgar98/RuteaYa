using System.Net;
using System.Net.Http.Json;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class PotencialServicio : IPotencialServicio
    {
        private readonly HttpClient _http;

        public PotencialServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<PotencialMensualDto?> ObtenerPotencialAsync(string mes, string criterio)
        {
            var respuesta = await _http.GetAsync($"api/potencial?mes={Uri.EscapeDataString(mes)}&criterio={Uri.EscapeDataString(criterio)}");

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<PotencialMensualDto>();
        }
    }
}