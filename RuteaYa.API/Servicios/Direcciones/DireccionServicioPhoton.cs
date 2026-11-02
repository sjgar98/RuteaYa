using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Direcciones
{
    public class DireccionesNoDisponiblesException : Exception
    {
        public DireccionesNoDisponiblesException(string mensaje) : base(mensaje)
        {
        }

        public DireccionesNoDisponiblesException(string mensaje, Exception interna) : base(mensaje, interna)
        {
        }
    }

    /// <summary>Búsqueda de direcciones con Photon (geocodificador de OpenStreetMap, sin key).</summary>
    public class DireccionServicioPhoton : IDireccionServicio
    {
        private static readonly TimeSpan VidaBusqueda = TimeSpan.FromMinutes(10);

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly DireccionesOpciones _opciones;

        public DireccionServicioPhoton(HttpClient http, IMemoryCache cache, IOptions<DireccionesOpciones> opciones)
        {
            _http = http;
            _cache = cache;
            _opciones = opciones.Value;
        }

        public async Task<List<DireccionDto>> BuscarAsync(string texto, CancellationToken cancelacion = default)
        {
            string consulta = texto.Trim();
            if (consulta.Length == 0)
            {
                return new List<DireccionDto>();
            }

            string clave = "direcciones:" + consulta.ToLowerInvariant();
            if (_cache.TryGetValue(clave, out List<DireccionDto>? guardadas) && guardadas != null)
            {
                return guardadas;
            }

            // El servicio público pide uso moderado: pocos resultados, sesgo a Buenos Aires y caché.
            string url = $"api?q={Uri.EscapeDataString(consulta)}&limit={_opciones.LimiteResultados}"
                + $"&lat={_opciones.SesgoLat.ToString(CultureInfo.InvariantCulture)}"
                + $"&lon={_opciones.SesgoLng.ToString(CultureInfo.InvariantCulture)}"
                + $"&bbox={_opciones.Bbox}";

            using var respuesta = await _http.GetAsync(url, cancelacion);
            if (!respuesta.IsSuccessStatusCode)
            {
                throw new DireccionesNoDisponiblesException($"Photon respondió {(int)respuesta.StatusCode}.");
            }

            string contenido = await respuesta.Content.ReadAsStringAsync(cancelacion);

            List<DireccionDto> direcciones;
            try
            {
                direcciones = LeerDirecciones(contenido);
            }
            catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException
                || ex is FormatException || ex is KeyNotFoundException)
            {
                throw new DireccionesNoDisponiblesException("Photon devolvió una respuesta inesperada.", ex);
            }

            _cache.Set(clave, direcciones, VidaBusqueda);
            return direcciones;
        }

        private List<DireccionDto> LeerDirecciones(string contenido)
        {
            var direcciones = new List<DireccionDto>();

            using var documento = JsonDocument.Parse(contenido);
            if (!documento.RootElement.TryGetProperty("features", out var resultados))
            {
                return direcciones;
            }

            foreach (var resultado in resultados.EnumerateArray())
            {
                var direccion = ADireccion(resultado);

                // La vista identifica la dirección por su texto: no se repiten textos.
                if (direccion != null && !direcciones.Any(d =>
                        string.Equals(d.Direccion, direccion.Direccion, StringComparison.OrdinalIgnoreCase)))
                {
                    direcciones.Add(direccion);
                }
            }

            return direcciones;
        }

        private DireccionDto? ADireccion(JsonElement resultado)
        {
            if (!resultado.TryGetProperty("geometry", out var geometria)
                || !geometria.TryGetProperty("coordinates", out var coordenadas)
                || coordenadas.GetArrayLength() < 2
                || !resultado.TryGetProperty("properties", out var propiedades))
            {
                return null;
            }

            string? pais = Texto(propiedades, "countrycode");
            if (pais != null && !string.Equals(pais, _opciones.CodigoPais, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string texto = ArmarTexto(propiedades);
            if (texto.Length == 0)
            {
                return null;
            }

            // GeoJSON viene como [longitud, latitud].
            return new DireccionDto
            {
                Direccion = texto,
                Lng = Math.Round((decimal)coordenadas[0].GetDouble(), 6),
                Lat = Math.Round((decimal)coordenadas[1].GetDouble(), 6)
            };
        }

        // "Obelisco, Avenida 9 de Julio 1, San Nicolás, Buenos Aires": lugar, calle y número, barrio, ciudad, provincia.
        private static string ArmarTexto(JsonElement propiedades)
        {
            string? nombre = Texto(propiedades, "name");
            string? calle = Texto(propiedades, "street");
            string? numero = Texto(propiedades, "housenumber");
            string? ciudad = Texto(propiedades, "city") ?? Texto(propiedades, "county");
            string? provincia = Texto(propiedades, "state");
            

            var partes = new List<string>();

            if (ciudad != null && ciudad.StartsWith("Comuna ", StringComparison.OrdinalIgnoreCase))
            {
                ciudad = null;
            }

            if (!string.IsNullOrEmpty(calle))
            {
                if (!string.IsNullOrEmpty(nombre) && !string.Equals(nombre, calle, StringComparison.OrdinalIgnoreCase))
                {
                    Agregar(partes, nombre);
                }
                Agregar(partes, string.IsNullOrEmpty(numero) ? calle : $"{calle} {numero}");
            }
            else
            {
                Agregar(partes, nombre);
            }

            Agregar(partes, Texto(propiedades, "district") ?? Texto(propiedades, "locality"));
            Agregar(partes, ciudad);
            Agregar(partes, string.Equals(provincia, "Ciudad Autónoma de Buenos Aires", StringComparison.OrdinalIgnoreCase)
                ? "CABA"
                : provincia);

            return string.Join(", ", partes);
        }

        private static void Agregar(List<string> partes, string? valor)
        {
            if (!string.IsNullOrWhiteSpace(valor)
                && !partes.Any(p => string.Equals(p, valor, StringComparison.OrdinalIgnoreCase)))
            {
                partes.Add(valor.Trim());
            }
        }

        private static string? Texto(JsonElement propiedades, string nombre)
        {
            return propiedades.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.String
                ? valor.GetString()?.Trim()
                : null;
        }
    }
}