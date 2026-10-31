using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Rutas;
using RuteaYa.DTOs.Rutas;
using Xunit;

namespace RuteaYa.Tests
{
    public class RutaServicioGoogleTests
    {
        private const string PolylineDeTresPuntos = "_p~iF~ps|U_ulLnnqC_mqNvxq`@";

        private static readonly CoordenadaDto Belgrano = new CoordenadaDto(-34.5627m, -58.4565m);
        private static readonly CoordenadaDto Lomas = new CoordenadaDto(-34.7609m, -58.4063m);
        private static readonly CoordenadaDto Palermo = new CoordenadaDto(-34.5889m, -58.4306m);
        private static readonly CoordenadaDto Caballito = new CoordenadaDto(-34.6189m, -58.4422m);
        private static readonly CoordenadaDto Avellaneda = new CoordenadaDto(-34.6627m, -58.3653m);
        private static readonly CoordenadaDto Quilmes = new CoordenadaDto(-34.7206m, -58.2546m);
        private static string RespuestaRuta(int metros, int segundos)
        {
            return $$$"""
        {"routes":[{"distanceMeters":{{{metros}}},"duration":"{{{segundos}}}s","polyline":{"encodedPolyline":"{{{PolylineDeTresPuntos}}}"}}]}
        """;
        }

        private const string RespuestaGeocodificacion = """
            {"status":"OK","results":[{"geometry":{"location":{"lat":-34.5627,"lng":-58.4565}}}]}
            """;

        private static RutaServicioGoogle CrearServicio(ManejadorFalso manejador)
        {
            var http = new HttpClient(manejador);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var opciones = Options.Create(new RutasOpciones { Proveedor = "Google", GoogleApiKey = "clave-de-prueba" });

            return new RutaServicioGoogle(http, cache, opciones, new RutaServicioMock(), NullLogger<RutaServicioGoogle>.Instance);
        }

        [Fact]
        public async Task CalcularRutaAsync_RespuestaValida_ParseaDistanciaDuracionYPolyline()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            var servicio = CrearServicio(manejador);

            var ruta = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal(18.4m, ruta.DistanciaKm);
            Assert.Equal(55, ruta.DuracionMinutos);
            Assert.Equal(PolylineDeTresPuntos, ruta.Polyline);
            Assert.Equal(3, ruta.PuntosRecorrido.Count);
        }

        [Fact]
        public async Task CalcularRutaAsync_DuracionConSegundosSobrantes_RedondeaHaciaArriba()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(1000, 61));
            var servicio = CrearServicio(manejador);

            var ruta = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal(2, ruta.DuracionMinutos);
        }

        [Fact]
        public async Task CalcularRutaAsync_MismoRecorridoDosVeces_HaceUnaSolaPeticion()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            var servicio = CrearServicio(manejador);

            var primera = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });
            var segunda = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal(1, manejador.CantidadPeticiones);
            Assert.Equal(primera.DistanciaKm, segunda.DistanciaKm);
        }

        [Fact]
        public async Task CalcularRutaAsync_RecorridosDistintos_NoComparteCache()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(21400, 3840));
            var servicio = CrearServicio(manejador);

            var sinParada = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });
            var conParada = await servicio.CalcularRutaAsync(new[] { Belgrano, Palermo, Lomas });

            Assert.Equal(2, manejador.CantidadPeticiones);
            Assert.NotEqual(sinParada.DistanciaKm, conParada.DistanciaKm);
        }

        [Fact]
        public async Task CalcularRutaAsync_EnviaLaKeyYElFieldMaskEnLosHeaders()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(1000, 60));
            var servicio = CrearServicio(manejador);

            await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal("clave-de-prueba", manejador.ClavesEnviadas.Single());
            Assert.Contains("routes.distanceMeters", manejador.FieldMasksEnviados.Single());
        }

        [Fact]
        public async Task CalcularRutaAsync_SinParadas_NoEnviaIntermediates()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(1000, 60));
            var servicio = CrearServicio(manejador);

            await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.DoesNotContain("intermediates", manejador.Cuerpos.Single());
        }

        [Fact]
        public async Task CalcularRutaAsync_ConParadas_EnviaLasParadasEnOrden()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(1000, 60));
            var servicio = CrearServicio(manejador);

            await servicio.CalcularRutaAsync(new[] { Belgrano, Palermo, Caballito, Lomas });

            string cuerpo = manejador.Cuerpos.Single();
            Assert.Contains("intermediates", cuerpo);
            Assert.True(cuerpo.IndexOf("-34.5889", StringComparison.Ordinal) < cuerpo.IndexOf("-34.6189", StringComparison.Ordinal));
        }

        [Fact]
        public async Task CalcularRutaAsync_GoogleRespondeError_LanzaRutaNoDisponible()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.Forbidden, "{\"error\":{\"message\":\"API key not valid\"}}");
            var servicio = CrearServicio(manejador);

            await Assert.ThrowsAsync<RutaNoDisponibleException>(
                () => servicio.CalcularRutaAsync(new[] { Belgrano, Lomas }));
        }

        [Fact]
        public async Task CalcularRutaAsync_ErrorNoSeCachea_LaProximaLlamadaReintenta()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.ServiceUnavailable, "{}");
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            var servicio = CrearServicio(manejador);

            await Assert.ThrowsAsync<RutaNoDisponibleException>(
                () => servicio.CalcularRutaAsync(new[] { Belgrano, Lomas }));
            var ruta = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal(18.4m, ruta.DistanciaKm);
            Assert.Equal(2, manejador.CantidadPeticiones);
        }

        [Fact]
        public async Task CalcularRutaAsync_RespuestaSinRutas_LanzaRutaNoDisponible()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, "{}");
            var servicio = CrearServicio(manejador);

            await Assert.ThrowsAsync<RutaNoDisponibleException>(
                () => servicio.CalcularRutaAsync(new[] { Belgrano, Lomas }));
        }

        [Fact]
        public async Task CalcularRutaAsync_RecorridoConUnSoloPunto_LanzaArgumentException()
        {
            var servicio = CrearServicio(new ManejadorFalso());

            await Assert.ThrowsAsync<ArgumentException>(
                () => servicio.CalcularRutaAsync(new[] { Belgrano }));
        }

        [Fact]
        public async Task CompararConEnvioAsync_CalculaDesvioYTiempoAdicional()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));   // habitual
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(21400, 3840));   // con el envío
            var servicio = CrearServicio(manejador);

            var detalle = await servicio.CompararConEnvioAsync(new[] { Belgrano, Lomas }, Palermo, Avellaneda);

            Assert.Equal(18.4m, detalle.DistanciaHabitualKm);
            Assert.Equal(21.4m, detalle.DistanciaConEnvioKm);
            Assert.Equal(3.0m, detalle.DesvioKm);
            Assert.Equal(55, detalle.TiempoHabitualMinutos);
            Assert.Equal(9, detalle.TiempoAdicionalMinutos);
            Assert.Equal(2, manejador.CantidadPeticiones);
        }

        [Fact]
        public async Task CompararConEnvioAsync_OtroEnvioDelMismoViaje_ReusaLaRutaHabitualDeLaCache()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));   // habitual (una sola vez)
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(21400, 3840));   // con el primer envío
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(19400, 3420));   // con el segundo envío
            var servicio = CrearServicio(manejador);
            var habitual = new[] { Belgrano, Lomas };

            await servicio.CompararConEnvioAsync(habitual, Palermo, Avellaneda);
            var segundo = await servicio.CompararConEnvioAsync(habitual, Caballito, Lomas);

            Assert.Equal(3, manejador.CantidadPeticiones);
            Assert.Equal(1.0m, segundo.DesvioKm);
        }

        [Fact]
        public async Task CompararConEnvioAsync_SiConEnvioDaMenosQueLaHabitual_DesvioNoEsNegativo()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18000, 3200));
            var servicio = CrearServicio(manejador);

            var detalle = await servicio.CompararConEnvioAsync(new[] { Belgrano, Lomas }, Palermo, Caballito);

            Assert.Equal(0m, detalle.DesvioKm);
            Assert.Equal(0, detalle.TiempoAdicionalMinutos);
        }

        [Fact]
        public async Task GeocodificarDireccionAsync_RespuestaOk_DevuelveLasCoordenadas()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaGeocodificacion);
            var servicio = CrearServicio(manejador);

            var coordenada = await servicio.GeocodificarDireccionAsync("Belgrano, CABA");

            Assert.Equal(-34.5627m, coordenada.Latitud);
            Assert.Equal(-58.4565m, coordenada.Longitud);
        }

        [Fact]
        public async Task GeocodificarDireccionAsync_MismaDireccionConOtraEscritura_UsaLaCache()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaGeocodificacion);
            var servicio = CrearServicio(manejador);

            await servicio.GeocodificarDireccionAsync("Belgrano, CABA");
            await servicio.GeocodificarDireccionAsync("  belgrano, caba ");

            Assert.Equal(1, manejador.CantidadPeticiones);
        }

        [Fact]
        public async Task GeocodificarDireccionAsync_SinResultados_LanzaRutaNoDisponible()
        {
            var manejador = new ManejadorFalso();
            manejador.Encolar(HttpStatusCode.OK, "{\"status\":\"ZERO_RESULTS\",\"results\":[]}");
            var servicio = CrearServicio(manejador);

            await Assert.ThrowsAsync<RutaNoDisponibleException>(
                () => servicio.GeocodificarDireccionAsync("Calle que no existe 123"));
        }


        // Handler que devuelve respuestas encoladas en orden y registra lo que recibió.
        private sealed class ManejadorFalso : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> _respuestas = new Queue<HttpResponseMessage>();

            public int CantidadPeticiones { get; private set; }
            public List<string> Cuerpos { get; } = new List<string>();
            public List<string> ClavesEnviadas { get; } = new List<string>();
            public List<string> FieldMasksEnviados { get; } = new List<string>();

            public void Encolar(HttpStatusCode estado, string json)
            {
                _respuestas.Enqueue(new HttpResponseMessage(estado)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
            {
                CantidadPeticiones++;

                
                Cuerpos.Add(peticion.Content == null ? string.Empty : await peticion.Content.ReadAsStringAsync(cancelacion));
                if (peticion.Headers.TryGetValues("X-Goog-Api-Key", out var claves))
                {
                    ClavesEnviadas.Add(string.Join(",", claves));
                }
                if (peticion.Headers.TryGetValues("X-Goog-FieldMask", out var mascaras))
                {
                    FieldMasksEnviados.Add(string.Join(",", mascaras));
                }

                if (_respuestas.Count == 0)
                {
                    throw new InvalidOperationException("Llegó una petición que el test no esperaba.");
                }
                return _respuestas.Dequeue();
            }
        }
    }
}