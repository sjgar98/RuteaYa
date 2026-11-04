using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Rutas
{
    public class RutaNoDisponibleException : Exception
    {
        public RutaNoDisponibleException(string mensaje) : base(mensaje)
        {
        }
    }

    /// <summary>Implementación real con Google Routes API (REST).</summary>
    public class RutaServicioGoogle : IRutaServicio
    {
        private const string UrlRutas = "https://routes.googleapis.com/directions/v2:computeRoutes";

        private static readonly TimeSpan VidaRuta = TimeSpan.FromHours(24);

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly RutasOpciones _opciones;
        private readonly ILogger<RutaServicioGoogle> _logger;

        public RutaServicioGoogle(
            HttpClient http,
            IMemoryCache cache,
            IOptions<RutasOpciones> opciones,
            ILogger<RutaServicioGoogle> logger)
        {
            _http = http;
            _cache = cache;
            _opciones = opciones.Value;
            _logger = logger;
        }

        public async Task<RutaResultadoDto> CalcularRutaAsync(
            IReadOnlyList<CoordenadaDto> recorrido, CancellationToken cancelacion = default)
        {
            if (recorrido.Count < 2)
            {
                throw new ArgumentException("El recorrido necesita al menos origen y destino.", nameof(recorrido));
            }

            string clave = ClaveRuta(recorrido);
            if (_cache.TryGetValue(clave, out RutaResultadoDto? enCache) && enCache != null)
            {
                return enCache;
            }

            var cuerpo = new Dictionary<string, object>
            {
                ["origin"] = Waypoint(recorrido[0]),
                ["destination"] = Waypoint(recorrido[^1]),
                ["travelMode"] = "DRIVE",
                // Sin tráfico: más barato y la ruta es estable, así se puede cachear.
                ["routingPreference"] = "TRAFFIC_UNAWARE"
            };
            if (recorrido.Count > 2)
            {
                cuerpo["intermediates"] = recorrido.Skip(1).Take(recorrido.Count - 2).Select(Waypoint).ToList();
            }

            using var peticion = new HttpRequestMessage(HttpMethod.Post, UrlRutas)
            {
                Content = JsonContent.Create(cuerpo)
            };
            peticion.Headers.Add("X-Goog-Api-Key", _opciones.GoogleApiKey);
            peticion.Headers.Add("X-Goog-FieldMask", "routes.distanceMeters,routes.duration,routes.polyline.encodedPolyline");

            using var respuesta = await _http.SendAsync(peticion, cancelacion);
            string contenido = await respuesta.Content.ReadAsStringAsync(cancelacion);

            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogWarning("Routes API respondió {Estado}: {Detalle}", (int)respuesta.StatusCode, contenido);
                throw new RutaNoDisponibleException($"Google Routes respondió {(int)respuesta.StatusCode}.");
            }

            using var documento = JsonDocument.Parse(contenido);
            if (!documento.RootElement.TryGetProperty("routes", out var rutas) || rutas.GetArrayLength() == 0)
            {
                throw new RutaNoDisponibleException("Google no encontró una ruta para el recorrido.");
            }

            var primera = rutas[0];
            int metros = primera.TryGetProperty("distanceMeters", out var m) ? m.GetInt32() : 0;
            string duracion = primera.TryGetProperty("duration", out var d) ? d.GetString() ?? "0s" : "0s";
            double segundos = double.Parse(duracion.TrimEnd('s'), CultureInfo.InvariantCulture);
            string codificada = primera.TryGetProperty("polyline", out var pl)
                && pl.TryGetProperty("encodedPolyline", out var ep)
                    ? ep.GetString() ?? string.Empty
                    : string.Empty;

            var resultado = new RutaResultadoDto
            {
                DistanciaKm = Math.Round(metros / 1000m, 2),
                DuracionMinutos = (int)Math.Ceiling(segundos / 60.0),
                Polyline = codificada,
                PuntosRecorrido = PolylineCodec.Decodificar(codificada)
            };

            _cache.Set(clave, resultado, VidaRuta);
            return resultado;
        }

        public Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(
            IReadOnlyList<CoordenadaDto> recorridoHabitual,
            CoordenadaDto retiro,
            CoordenadaDto entrega,
            CancellationToken cancelacion = default)
        {
            return ComparadorRutas.CompararAsync(CalcularRutaAsync, recorridoHabitual, retiro, entrega, cancelacion);
        }

        private static object Waypoint(CoordenadaDto c)
        {
            return new { location = new { latLng = new { latitude = (double)c.Latitud, longitude = (double)c.Longitud } } };
        }

        private static string ClaveRuta(IReadOnlyList<CoordenadaDto> recorrido)
        {
            var puntos = recorrido.Select(c =>
                c.Latitud.ToString("F5", CultureInfo.InvariantCulture) + "," + c.Longitud.ToString("F5", CultureInfo.InvariantCulture));
            return "ruta:" + string.Join(";", puntos);
        }
    }
}