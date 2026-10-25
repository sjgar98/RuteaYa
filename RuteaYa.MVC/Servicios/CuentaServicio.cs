using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Servicios
{
    public class CuentaServicio : ICuentaServicio
    {
        private readonly HttpClient _http;

        public CuentaServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<SesionDto?> LoginAsync(LoginDto dto)
        {
            var respuesta = await _http.PostAsJsonAsync("api/usuarios/login", dto);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<SesionDto>();
        }

        public async Task<(SesionDto? Sesion, Dictionary<string, string[]> Errores)> RegistrarAsync(RegistroUsuarioDto dto)
        {
            var respuesta = await _http.PostAsJsonAsync("api/usuarios/registro", dto);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                return (null, new Dictionary<string, string[]>(problema?.Errors ?? new Dictionary<string, string[]>()));
            }

            // Email repetido: se muestra debajo del campo Email.
            if (respuesta.StatusCode == HttpStatusCode.Conflict)
            {
                var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                var mensaje = problema?.Title ?? "Ya existe una cuenta con ese email.";
                return (null, new Dictionary<string, string[]> { [nameof(RegistroUsuarioDto.Email)] = new[] { mensaje } });
            }

            respuesta.EnsureSuccessStatusCode();
            return (await respuesta.Content.ReadFromJsonAsync<SesionDto>(), new Dictionary<string, string[]>());
        }
    }
}
