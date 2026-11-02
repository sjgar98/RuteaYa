using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RuteaYa.API.Servicios.Rutas;
using RuteaYa.DTOs.Rutas;
using Xunit;

namespace RuteaYa.Tests
{
    public class RutaServicioOsrmTests
    {
        private const string PolylineDeTresPuntos = "_p~iF~ps|U_ulLnnqC_mqNvxq`@";

        private static readonly CoordenadaDto Belgrano = new CoordenadaDto(-34.5627m, -58.4565m);
        private static readonly CoordenadaDto Lomas = new CoordenadaDto(-34.7609m, -58.4063m);
        private static readonly CoordenadaDto Palermo = new CoordenadaDto(-34.5889m, -58.4306m);
        private static readonly CoordenadaDto Caballito = new CoordenadaDto(-34.6189m, -58.4422m);
        private static readonly CoordenadaDto Avellaneda = new CoordenadaDto(-34.6627m, -58.3653m);

        private static string RespuestaRuta(int metros, int segundos)
        {
            return "{\"code\":\"Ok\",\"routes\":[{\"distance\":" + metros + ",\"duration\":" + segundos
                + ",\"geometry\":\"" + PolylineDeTresPuntos + "\"}],\"waypoints\":[]}";
        }

        // Sin espera entre consultas: los tests no tienen que demorar 1 s por request.
        private static RutaServicioOsrm CrearServicio(ManejadorHttpFalso manejador, int intervaloMs = 0)
        {
            var http = new HttpClient(manejador) { BaseAddress = new Uri("https://router.project-osrm.org/") };
            var opciones = Options.Create(new RutasOpciones { OsrmIntervaloMs = intervaloMs });
            return new RutaServicioOsrm(http, new MemoryCache(new MemoryCacheOptions()), opciones);
        }

        [Fact]
        public async Task CalcularRutaAsync_RespuestaValida_ParseaDistanciaDuracionYPolyline()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3299));

            var ruta = await CrearServicio(manejador).CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal(18.4m, ruta.DistanciaKm);
            Assert.Equal(55, ruta.DuracionMinutos);
            Assert.Equal(PolylineDeTresPuntos, ruta.Polyline);
            Assert.Equal(3, ruta.PuntosRecorrido.Count);
        }

        [Fact]
        public async Task CalcularRutaAsync_EnviaLongitudLatitudEnOrdenYPidePolyline()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(1000, 60));

            await CrearServicio(manejador).CalcularRutaAsync(new[] { Belgrano, Palermo, Lomas });

            string url = manejador.Urls.Single();
            Assert.Contains("route/v1/driving/-58.456500,-34.562700;-58.430600,-34.588900;-58.406300,-34.760900", url);
            Assert.Contains("overview=full", url);
            Assert.Contains("geometries=polyline", url);
        }

        [Fact]
        public async Task CalcularRutaAsync_MismoRecorridoDosVeces_HaceUnaSolaPeticion()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            var servicio = CrearServicio(manejador);

            await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });
            await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal(1, manejador.CantidadPeticiones);
        }

        [Fact]
        public async Task CalcularRutaAsync_RecorridosDistintos_NoComparteCache()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(21400, 3840));
            var servicio = CrearServicio(manejador);

            var sinParada = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });
            var conParada = await servicio.CalcularRutaAsync(new[] { Belgrano, Palermo, Lomas });

            Assert.Equal(2, manejador.CantidadPeticiones);
            Assert.NotEqual(sinParada.DistanciaKm, conParada.DistanciaKm);
        }

        [Fact]
        public async Task CalcularRutaAsync_RutaImposible400_LanzaRutaNoDisponible()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.BadRequest, "{\"code\":\"NoRoute\",\"message\":\"Impossible route between points\"}");

            await Assert.ThrowsAsync<RutaNoDisponibleException>(
                () => CrearServicio(manejador).CalcularRutaAsync(new[] { Belgrano, Lomas }));
        }

        [Fact]
        public async Task CalcularRutaAsync_CodigoNoOkConStatus200_LanzaRutaNoDisponible()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, "{\"code\":\"NoRoute\",\"routes\":[]}");

            await Assert.ThrowsAsync<RutaNoDisponibleException>(
                () => CrearServicio(manejador).CalcularRutaAsync(new[] { Belgrano, Lomas }));
        }

        [Fact]
        public async Task CalcularRutaAsync_RespuestaQueNoEsJson_LanzaRutaNoDisponible()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, "<html>mantenimiento</html>");

            await Assert.ThrowsAsync<RutaNoDisponibleException>(
                () => CrearServicio(manejador).CalcularRutaAsync(new[] { Belgrano, Lomas }));
        }

        [Theory]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task CalcularRutaAsync_ServicioSaturadoOCaido_LanzaHttpRequestException(HttpStatusCode estado)
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(estado, "{}");

            await Assert.ThrowsAsync<HttpRequestException>(
                () => CrearServicio(manejador).CalcularRutaAsync(new[] { Belgrano, Lomas }));
        }

        [Fact]
        public async Task CalcularRutaAsync_ErrorNoSeCachea_LaProximaLlamadaReintenta()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.ServiceUnavailable, "{}");
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            var servicio = CrearServicio(manejador);

            await Assert.ThrowsAsync<HttpRequestException>(() => servicio.CalcularRutaAsync(new[] { Belgrano, Lomas }));
            var ruta = await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });

            Assert.Equal(18.4m, ruta.DistanciaKm);
            Assert.Equal(2, manejador.CantidadPeticiones);
        }

        [Fact]
        public async Task CalcularRutaAsync_RecorridoConUnSoloPunto_LanzaArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => CrearServicio(new ManejadorHttpFalso()).CalcularRutaAsync(new[] { Belgrano }));
        }

        [Fact]
        public async Task CalcularRutaAsync_ConsultasSeguidas_RespetanElIntervaloMinimo()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(21400, 3840));
            var servicio = CrearServicio(manejador, intervaloMs: 250);

            var reloj = Stopwatch.StartNew();
            await servicio.CalcularRutaAsync(new[] { Belgrano, Lomas });
            await servicio.CalcularRutaAsync(new[] { Belgrano, Palermo, Lomas });
            reloj.Stop();

            Assert.True(reloj.ElapsedMilliseconds >= 200, $"Tardó {reloj.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task CompararConEnvioAsync_CalculaDesvioYTiempoAdicional()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(21400, 3840));

            var detalle = await CrearServicio(manejador).CompararConEnvioAsync(new[] { Belgrano, Lomas }, Palermo, Avellaneda);

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
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(18400, 3300));
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(21400, 3840));
            manejador.Encolar(HttpStatusCode.OK, RespuestaRuta(19400, 3420));
            var servicio = CrearServicio(manejador);
            var habitual = new[] { Belgrano, Lomas };

            await servicio.CompararConEnvioAsync(habitual, Palermo, Avellaneda);
            var segundo = await servicio.CompararConEnvioAsync(habitual, Caballito, Lomas);

            Assert.Equal(3, manejador.CantidadPeticiones);
            Assert.Equal(1.0m, segundo.DesvioKm);
        }

        [Fact]
        public async Task GeocodificarDireccionAsync_NoEstaSoportado()
        {
            await Assert.ThrowsAsync<NotSupportedException>(
                () => CrearServicio(new ManejadorHttpFalso()).GeocodificarDireccionAsync("Belgrano"));
        }
    }
}