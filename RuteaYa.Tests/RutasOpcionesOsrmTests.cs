using RuteaYa.API.Servicios.Rutas;
using Xunit;

namespace RuteaYa.Tests
{
    public class RutasOpcionesOsrmTests
    {
        [Theory]
        [InlineData("Auto", "", true)]
        [InlineData("auto", "   ", true)]
        [InlineData("Auto", "clave", false)]
        [InlineData("Osrm", "", true)]
        [InlineData("osrm", "clave", true)]
        [InlineData("Google", "clave", false)]
        [InlineData("Mock", "", false)]
        [InlineData("Otro", "", false)]
        public void UsarOsrm_SegunProveedorYKey(string proveedor, string key, bool esperado)
        {
            var opciones = new RutasOpciones { Proveedor = proveedor, GoogleApiKey = key };

            Assert.Equal(esperado, opciones.UsarOsrm());
        }

        [Theory]
        [InlineData("Auto", "")]
        [InlineData("Auto", "clave")]
        [InlineData("Osrm", "clave")]
        [InlineData("Google", "clave")]
        [InlineData("Mock", "clave")]
        public void UsarGoogleYUsarOsrm_NuncaSeActivanALaVez(string proveedor, string key)
        {
            var opciones = new RutasOpciones { Proveedor = proveedor, GoogleApiKey = key };

            Assert.False(opciones.UsarGoogle() && opciones.UsarOsrm());
        }

        [Fact]
        public void ValoresPorDefecto_RespetanLaPoliticaDelServidorPublico()
        {
            var opciones = new RutasOpciones();

            Assert.Equal("https://router.project-osrm.org", opciones.OsrmUrl);
            Assert.Equal(1000, opciones.OsrmIntervaloMs);
            Assert.True(opciones.UsarOsrm());
        }
    }
}