using System.Net;
using System.Net.Http.Json;
using RuteaYa.DTOs;
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

        public async Task<List<OportunidadDto>> ObtenerOportunidadesAsync(int viajeId, string? ordenarPor = "ganancia")
        {
            var url = string.IsNullOrEmpty(ordenarPor) 
                ? $"api/oportunidades/{viajeId}"
                : $"api/oportunidades/{viajeId}?ordenarPor={Uri.EscapeDataString(ordenarPor)}";

            try
            {
                return await _http.GetFromJsonAsync<List<OportunidadDto>>(url) ?? new List<OportunidadDto>();
            }
            catch
            {
                // Si falla, retornar lista vacía para que la vista lo maneje
                return new List<OportunidadDto>();
            }
        }

        public async Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int viajeId, int envioId)
        {
            var url = $"api/oportunidades/{viajeId}/detalle?envioId={envioId}";

            var respuesta = await _http.GetAsync(url);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            return await respuesta.Content.ReadFromJsonAsync<DetalleOportunidadDto>();
        }

        public async Task<ResultadoAceptarOportunidad> AceptarAsync(int viajeId, int envioId)
        {
            var dto = new AceptarOportunidadDto { EnvioId = envioId };
            var url = $"api/oportunidades/{viajeId}/aceptar";

            var respuesta = await _http.PostAsJsonAsync(url, dto);

            switch (respuesta.StatusCode)
            {
                case HttpStatusCode.Created:
                    return new ResultadoAceptarOportunidad { Exitoso = true };

                case HttpStatusCode.NotFound:
                    return new ResultadoAceptarOportunidad { NoEncontrado = true };

                case HttpStatusCode.Conflict:
                    // Intentar leer el mensaje de conflicto
                    var conflictoMsg = "No se pudo aceptar la oportunidad por un conflicto.";
                    try
                    {
                        var contenido = await respuesta.Content.ReadFromJsonAsync<dynamic>();
                        if (contenido?.mensaje != null)
                            conflictoMsg = contenido.mensaje;
                    }
                    catch { }
                    return new ResultadoAceptarOportunidad { Conflicto = conflictoMsg };

                case HttpStatusCode.BadRequest:
                    var badRequestMsg = "Datos inválidos o conflicto de capacidad/desvío.";
                    try
                    {
                        var contenido = await respuesta.Content.ReadFromJsonAsync<dynamic>();
                        if (contenido?.mensaje != null)
                            badRequestMsg = contenido.mensaje;
                    }
                    catch { }
                    return new ResultadoAceptarOportunidad { Conflicto = badRequestMsg };

                default:
                    return new ResultadoAceptarOportunidad { Conflicto = "Error desconocido al aceptar la oportunidad." };
            }
        }
    }
}
