using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Rutas;
using RuteaYa.DTOs.Rutas;
using Xunit;

namespace RuteaYa.Tests
{
    public class RutasTests
    {
        [Fact]
        public void PolylineCodec_Codificar_CoincideConElEjemploOficialDeGoogle()
        {
            var puntos = new List<CoordenadaDto>
            {
                new CoordenadaDto(38.5m, -120.2m),
                new CoordenadaDto(40.7m, -120.95m),
                new CoordenadaDto(43.252m, -126.453m)
            };

            Assert.Equal("_p~iF~ps|U_ulLnnqC_mqNvxq`@", PolylineCodec.Codificar(puntos));
        }

        [Fact]
        public void PolylineCodec_Decodificar_DevuelveLosPuntosOriginales()
        {
            var puntos = PolylineCodec.Decodificar("_p~iF~ps|U_ulLnnqC_mqNvxq`@");

            Assert.Equal(3, puntos.Count);
            Assert.Equal(38.5m, puntos[0].Latitud);
            Assert.Equal(-126.453m, puntos[2].Longitud);
        }

        [Fact]
        public void InsercionRecorrido_PoneElRetiroYLaEntregaDondeMenosDesvian()
        {
            var a = new CoordenadaDto(0m, 0m);
            var b = new CoordenadaDto(0m, 0.1m);
            var c = new CoordenadaDto(0m, 0.2m);
            var retiro = new CoordenadaDto(0.02m, 0.1m);
            var entrega = new CoordenadaDto(0.02m, 0.15m);

            var resultado = InsercionRecorrido.Insertar(new[] { a, b, c }, retiro, entrega);

            Assert.Equal(new[] { a, b, retiro, entrega, c }, resultado);
        }

        [Fact]
        public void InsercionRecorrido_SiempreRetiraAntesDeEntregar()
        {
            var a = new CoordenadaDto(0m, 0m);
            var b = new CoordenadaDto(0m, 0.1m);
            var c = new CoordenadaDto(0m, 0.2m);
            // El retiro queda cerca del final y la entrega cerca del principio: igual debe respetar el orden.
            var retiro = new CoordenadaDto(0.001m, 0.19m);
            var entrega = new CoordenadaDto(0.001m, 0.01m);

            var resultado = InsercionRecorrido.Insertar(new[] { a, b, c }, retiro, entrega);

            Assert.True(resultado.IndexOf(retiro) < resultado.IndexOf(entrega));
            Assert.Same(a, resultado[0]);
            Assert.Same(c, resultado[^1]);
        }

        [Fact]
        public async Task Mock_CompararConEnvio_UnEnvioQueDesviaMasTieneMasDesvio()
        {
            var mock = new RutaServicioMock();
            var belgrano = new CoordenadaDto(-34.5627m, -58.4565m);
            var lomas = new CoordenadaDto(-34.7609m, -58.4063m);
            var caballito = new CoordenadaDto(-34.6189m, -58.4422m);
            var moron = new CoordenadaDto(-34.6534m, -58.6198m);
            var quilmes = new CoordenadaDto(-34.7206m, -58.2546m);
            var habitual = new[] { belgrano, lomas };

            var cerca = await mock.CompararConEnvioAsync(habitual, caballito, lomas);
            var lejos = await mock.CompararConEnvioAsync(habitual, moron, quilmes);

            Assert.True(cerca.DesvioKm < lejos.DesvioKm);
            Assert.True(lejos.TiempoAdicionalMinutos > cerca.TiempoAdicionalMinutos);
            Assert.Equal(cerca.DistanciaHabitualKm, lejos.DistanciaHabitualKm);
        }

        [Fact]
        public void GeoUtil_DistanciaMinimaARuta_UsaElVerticeMasCercano()
        {
            var ruta = new List<CoordenadaDto>
            {
                new CoordenadaDto(0m, 0m),
                new CoordenadaDto(0m, 0.1m)
            };

            decimal distancia = GeoUtil.DistanciaMinimaARutaKm(new CoordenadaDto(0m, 0.1m), ruta);

            Assert.Equal(0m, distancia);
        }
    }
}