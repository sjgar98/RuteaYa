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

    /// <summary>Implementación real con Google Routes API y Geocoding API (REST).</summary>
    public class RutaServicioGoogle : IRutaServicio
    {
        private const string UrlRutas = "https://routes.googleapis.com/directions/v2:computeRoutes";
        private const string UrlGeocodificacion = "https://maps.googleapis.com/maps/api/geocode/json";

        private static readonly TimeSpan VidaRuta = TimeSpan.FromHours(24);
        private static readonly TimeSpan VidaGeocodificacion = TimeSpan.FromDays(7);

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly RutasOpciones _opciones;
        private readonly RutaServicioMock _mock;
        private readonly ILogger<RutaServicioGoogle> _logger;

        public RutaServicioGoogle(
            HttpClient http,
            IMemoryCache cache,
            IOptions<RutasOpciones> opciones,
            RutaServicioMock mock,
            ILogger<RutaServicioGoogle> logger)
        {
            _http = http;
            _cache = cache;
            _opciones = opciones.Value;
            _mock = mock;
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

        public async Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion)
        {
            string clave = "geo:" + direccion.Trim().ToLowerInvariant();
            if (_cache.TryGetValue(clave, out CoordenadaDto? guardada) && guardada != null)
            {
                return guardada;
            }

            string url = $"{UrlGeocodificacion}?address={Uri.EscapeDataString(direccion)}"
                + $"&components=country:AR&key={Uri.EscapeDataString(_opciones.GoogleApiKey)}";

            using var respuesta = await _http.GetAsync(url);
            if (!respuesta.IsSuccessStatusCode)
            {
                throw new RutaNoDisponibleException($"Google Geocoding respondió {(int)respuesta.StatusCode}.");
            }

            using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            string estado = documento.RootElement.GetProperty("status").GetString() ?? string.Empty;
            if (estado != "OK")
            {
                throw new RutaNoDisponibleException($"No se pudo geocodificar \"{direccion}\" (estado {estado}).");
            }

            var ubicacion = documento.RootElement.GetProperty("results")[0].GetProperty("geometry").GetProperty("location");
            var coordenada = new CoordenadaDto(ubicacion.GetProperty("lat").GetDecimal(), ubicacion.GetProperty("lng").GetDecimal());

            _cache.Set(clave, coordenada, VidaGeocodificacion);
            return coordenada;
        }

        // --- Métodos de texto (legado): geocodifican y delegan en los de coordenadas. ---
        // TODO Servicio de rutas: borrarlos cuando ViajeServicio y OportunidadesServicio migren a coordenadas.

        public async Task<RutaResultadoDto> CalcularRutaConParadasAsync(List<string> paradas)
        {
            var coordenadas = new List<CoordenadaDto>();
            foreach (var direccion in paradas)
            {
                coordenadas.Add(await GeocodificarDireccionAsync(direccion));
            }
            return await CalcularRutaAsync(coordenadas);
        }

        public async Task<RutaResultadoDto> CalcularRutaAlternativaEnvioAsync(
            string origen, string destino, string retiroPaquete, string entregaPaquete)
        {
            var habitual = new List<CoordenadaDto>
            {
                await GeocodificarDireccionAsync(origen),
                await GeocodificarDireccionAsync(destino)
            };
            var retiro = await GeocodificarDireccionAsync(retiroPaquete);
            var entrega = await GeocodificarDireccionAsync(entregaPaquete);

            return await CalcularRutaAsync(InsercionRecorrido.Insertar(habitual, retiro, entrega));
        }

        public Task<decimal> CalcularDesvioAsync(string rutaOriginalId, string paradaExtra)
        {
            throw new NotSupportedException("Usar CompararConEnvioAsync: el desvío sale de comparar dos rutas.");
        }

        public async Task<DetalleOportunidadRutaDto> ObtenerDetalleOportunidadAsync(
            string origenHabitual, string destinoHabitual, string retiroEnvio, string entregaEnvio)
        {
            var habitual = new List<CoordenadaDto>
            {
                await GeocodificarDireccionAsync(origenHabitual),
                await GeocodificarDireccionAsync(destinoHabitual)
            };
            var retiro = await GeocodificarDireccionAsync(retiroEnvio);
            var entrega = await GeocodificarDireccionAsync(entregaEnvio);

            return await CompararConEnvioAsync(habitual, retiro, entrega);
        }

        public Task<List<string>> BuscarDireccionesAsync(string texto)
        {
            // TODO Servicio de mapas: Places Autocomplete. Por ahora, la lista fija del mock.
            return _mock.BuscarDireccionesAsync(texto);
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