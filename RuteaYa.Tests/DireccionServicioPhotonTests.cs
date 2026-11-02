using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RuteaYa.API.Servicios.Direcciones;
using Xunit;

namespace RuteaYa.Tests
{
    public class DireccionServicioPhotonTests
    {
        private const string RespuestaDosResultados = """
            {"type":"FeatureCollection","features":[
              {"geometry":{"coordinates":[-58.4565,-34.5627],"type":"Point"},"type":"Feature",
               "properties":{"name":"Belgrano","type":"district","city":"Buenos Aires","state":"Buenos Aires","countrycode":"AR"}},
              {"geometry":{"coordinates":[-58.3816,-34.6037],"type":"Point"},"type":"Feature",
               "properties":{"name":"Obelisco","street":"Avenida 9 de Julio","housenumber":"1","district":"San Nicolás","city":"Buenos Aires","state":"Buenos Aires","countrycode":"AR"}}
            ]}
            """;

        private static DireccionServicioPhoton CrearServicio(ManejadorHttpFalso manejador)
        {
            var http = new HttpClient(manejador) { BaseAddress = new Uri("https://photon.komoot.io/") };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("RuteaYa-TP/1.0");
            return new DireccionServicioPhoton(http, new MemoryCache(new MemoryCacheOptions()), Options.Create(new DireccionesOpciones()));
        }

        [Fact]
        public async Task BuscarAsync_RespuestaValida_ArmaTextoYCoordenadasEnElOrdenCorrecto()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaDosResultados);

            var direcciones = await CrearServicio(manejador).BuscarAsync("belgrano");

            Assert.Equal(2, direcciones.Count);
            Assert.Equal("Belgrano, Buenos Aires", direcciones[0].Direccion);
            // GeoJSON trae [longitud, latitud]: la latitud es la segunda.
            Assert.Equal(-34.5627m, direcciones[0].Lat);
            Assert.Equal(-58.4565m, direcciones[0].Lng);
            Assert.Equal("Obelisco, Avenida 9 de Julio 1, San Nicolás, Buenos Aires", direcciones[1].Direccion);
        }

        [Fact]
        public async Task BuscarAsync_EnviaTextoEscapadoLimiteSesgoYBbox()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, "{\"features\":[]}");

            await CrearServicio(manejador).BuscarAsync("Rivadavia 1234");

            string url = manejador.Urls.Single();
            Assert.StartsWith("https://photon.komoot.io/api?", url);
            Assert.Contains("q=Rivadavia%201234", url);
            Assert.Contains("limit=5", url);
            Assert.Contains("lat=-34.6037", url);
            Assert.Contains("lon=-58.3816", url);
            Assert.Contains("bbox=-59.1,-35.1,-57.85,-34.2", url);
            Assert.Contains("RuteaYa-TP", manejador.UserAgents.Single());
        }

        [Fact]
        public async Task BuscarAsync_MismoTextoConOtraEscritura_UsaLaCache()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, RespuestaDosResultados);
            var servicio = CrearServicio(manejador);

            await servicio.BuscarAsync("belgrano");
            await servicio.BuscarAsync("  BELGRANO ");

            Assert.Equal(1, manejador.CantidadPeticiones);
        }

        [Fact]
        public async Task BuscarAsync_ResultadosDeOtroPais_SeDescartan()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, """
                {"features":[
                  {"geometry":{"coordinates":[-57.84,-34.47],"type":"Point"},
                   "properties":{"name":"Colonia del Sacramento","countrycode":"UY"}},
                  {"geometry":{"coordinates":[-58.4565,-34.5627],"type":"Point"},
                   "properties":{"name":"Belgrano","countrycode":"AR"}}
                ]}
                """);

            var direcciones = await CrearServicio(manejador).BuscarAsync("colonia");

            Assert.Equal("Belgrano", direcciones.Single().Direccion);
        }

        [Fact]
        public async Task BuscarAsync_ResultadosSinGeometriaOSinTexto_SeDescartan()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, """
                {"features":[
                  {"properties":{"name":"Sin geometría","countrycode":"AR"}},
                  {"geometry":{"coordinates":[-58.4,-34.6],"type":"Point"},"properties":{"countrycode":"AR"}},
                  {"geometry":{"coordinates":[-58.4565,-34.5627],"type":"Point"},"properties":{"name":"Belgrano","countrycode":"AR"}}
                ]}
                """);

            var direcciones = await CrearServicio(manejador).BuscarAsync("bel");

            Assert.Equal("Belgrano", direcciones.Single().Direccion);
        }

        [Fact]
        public async Task BuscarAsync_ResultadosConElMismoTexto_NoSeRepiten()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, """
                {"features":[
                  {"geometry":{"coordinates":[-58.4565,-34.5627],"type":"Point"},"properties":{"name":"Belgrano","countrycode":"AR"}},
                  {"geometry":{"coordinates":[-58.4500,-34.5600],"type":"Point"},"properties":{"name":"belgrano","countrycode":"AR"}}
                ]}
                """);

            var direcciones = await CrearServicio(manejador).BuscarAsync("belgrano");

            Assert.Single(direcciones);
        }

        [Fact]
        public async Task BuscarAsync_SinResultados_DevuelveListaVacia()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, "{\"features\":[]}");

            var direcciones = await CrearServicio(manejador).BuscarAsync("zzzz");

            Assert.Empty(direcciones);
        }

        [Theory]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task BuscarAsync_PhotonRespondeError_LanzaDireccionesNoDisponibles(HttpStatusCode estado)
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(estado, "{}");

            await Assert.ThrowsAsync<DireccionesNoDisponiblesException>(
                () => CrearServicio(manejador).BuscarAsync("belgrano"));
        }

        [Fact]
        public async Task BuscarAsync_RespuestaQueNoEsJson_LanzaDireccionesNoDisponibles()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.OK, "<html>mantenimiento</html>");

            await Assert.ThrowsAsync<DireccionesNoDisponiblesException>(
                () => CrearServicio(manejador).BuscarAsync("belgrano"));
        }

        [Fact]
        public async Task BuscarAsync_ErrorNoSeCachea_LaProximaBusquedaReintenta()
        {
            var manejador = new ManejadorHttpFalso();
            manejador.Encolar(HttpStatusCode.TooManyRequests, "{}");
            manejador.Encolar(HttpStatusCode.OK, RespuestaDosResultados);
            var servicio = CrearServicio(manejador);

            await Assert.ThrowsAsync<DireccionesNoDisponiblesException>(() => servicio.BuscarAsync("belgrano"));
            var direcciones = await servicio.BuscarAsync("belgrano");

            Assert.Equal(2, direcciones.Count);
            Assert.Equal(2, manejador.CantidadPeticiones);
        }

        [Fact]
        public async Task BuscarAsync_TextoVacio_NoLlamaAPhoton()
        {
            var manejador = new ManejadorHttpFalso();

            var direcciones = await CrearServicio(manejador).BuscarAsync("   ");

            Assert.Empty(direcciones);
            Assert.Equal(0, manejador.CantidadPeticiones);
        }
    }
}