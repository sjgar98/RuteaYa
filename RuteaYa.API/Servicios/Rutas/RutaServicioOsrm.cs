using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>Rutas con OSRM (OpenStreetMap, sin key). El servidor público es de uso moderado.</summary>
    public class RutaServicioOsrm : IRutaServicio
    {
        private static readonly TimeSpan VidaRuta = TimeSpan.FromHours(24);

        // El servidor público pide como máximo 1 request por segundo: una sola consulta a la vez y espaciadas.
        private static readonly SemaphoreSlim Turno = new SemaphoreSlim(1, 1);
        private static DateTime _ultimaPeticion = DateTime.MinValue;

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly RutasOpciones _opciones;

        public RutaServicioOsrm(HttpClient http, IMemoryCache cache, IOptions<RutasOpciones> opciones)
        {
            _http = http;
            _cache = cache;
            _opciones = opciones.Value;
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

            // OSRM recibe las coordenadas como longitud,latitud (al revés que CoordenadaDto) y las paradas en orden.
            string coordenadas = string.Join(";", recorrido.Select(c => $"{Numero(c.Longitud)},{Numero(c.Latitud)}"));
            string url = $"route/v1/driving/{coordenadas}?overview=full&geometries=polyline&steps=false&alternatives=false";

            await Turno.WaitAsync(cancelacion);
            try
            {
                // Otra consulta igual pudo terminar mientras esperábamos el turno.
                if (_cache.TryGetValue(clave, out RutaResultadoDto? calculadaMientras) && calculadaMientras != null)
                {
                    return calculadaMientras;
                }

                var espera = _ultimaPeticion + TimeSpan.FromMilliseconds(_opciones.OsrmIntervaloMs) - DateTime.UtcNow;
                if (espera > TimeSpan.Zero)
                {
                    await Task.Delay(espera, cancelacion);
                }
                _ultimaPeticion = DateTime.UtcNow;

                using var respuesta = await _http.GetAsync(url, cancelacion);
                string contenido = await respuesta.Content.ReadAsStringAsync(cancelacion);
                int estado = (int)respuesta.StatusCode;

                // Saturado o caído: es un problema del servicio (el respaldo lo pausa un rato).
                if (estado == 429 || estado >= 500)
                {
                    throw new HttpRequestException($"OSRM respondió {estado}.", null, respuesta.StatusCode);
                }

                // 4xx: la consulta no tiene ruta o es inválida.
                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new RutaNoDisponibleException($"OSRM respondió {estado}: {ResumenDelError(contenido)}");
                }

                var resultado = LeerRuta(contenido);
                _cache.Set(clave, resultado, VidaRuta);
                return resultado;
            }
            finally
            {
                Turno.Release();
            }
        }

        public Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(
            IReadOnlyList<CoordenadaDto> recorridoHabitual,
            CoordenadaDto retiro,
            CoordenadaDto entrega,
            CancellationToken cancelacion = default)
        {
            return ComparadorRutas.CompararAsync(CalcularRutaAsync, recorridoHabitual, retiro, entrega, cancelacion);
        }

        // OSRM no geocodifica: las direcciones se buscan con IDireccionServicio.
        public Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion)
        {
            throw new NotSupportedException("OSRM no geocodifica: usar IDireccionServicio.");
        }

        public Task<List<string>> BuscarDireccionesAsync(string texto)
        {
            throw new NotSupportedException("OSRM no busca direcciones: usar IDireccionServicio.");
        }

        private static RutaResultadoDto LeerRuta(string contenido)
        {
            try
            {
                using var documento = JsonDocument.Parse(contenido);
                var raiz = documento.RootElement;

                string codigo = raiz.TryGetProperty("code", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                if (codigo != "Ok" || !raiz.TryGetProperty("routes", out var rutas) || rutas.GetArrayLength() == 0)
                {
                    throw new RutaNoDisponibleException($"OSRM no encontró una ruta (código {codigo}).");
                }

                var primera = rutas[0];
                double metros = primera.GetProperty("distance").GetDouble();
                double segundos = primera.GetProperty("duration").GetDouble();
                string codificada = primera.TryGetProperty("geometry", out var g) && g.ValueKind == JsonValueKind.String
                    ? g.GetString() ?? string.Empty
                    : string.Empty;

                // Con geometries=polyline el formato es el mismo de Google (precisión 5).
                return new RutaResultadoDto
                {
                    DistanciaKm = Math.Round((decimal)metros / 1000m, 2),
                    DuracionMinutos = (int)Math.Ceiling(segundos / 60.0),
                    Polyline = codificada,
                    PuntosRecorrido = PolylineCodec.Decodificar(codificada)
                };
            }
            catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException
                || ex is KeyNotFoundException || ex is FormatException)
            {
                throw new RutaNoDisponibleException("OSRM devolvió una respuesta inesperada.");
            }
        }

        private static string ResumenDelError(string contenido)
        {
            try
            {
                using var documento = JsonDocument.Parse(contenido);
                string codigo = documento.RootElement.TryGetProperty("code", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                string mensaje = documento.RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? string.Empty : string.Empty;
                return $"{codigo} {mensaje}".Trim();
            }
            catch (JsonException)
            {
                return "respuesta no válida";
            }
        }

        private static string Numero(decimal valor)
        {
            return valor.ToString("F6", CultureInfo.InvariantCulture);
        }

        private static string ClaveRuta(IReadOnlyList<CoordenadaDto> recorrido)
        {
            return "osrm:" + string.Join(";", recorrido.Select(c => $"{Numero(c.Latitud)},{Numero(c.Longitud)}"));
        }
    }
}