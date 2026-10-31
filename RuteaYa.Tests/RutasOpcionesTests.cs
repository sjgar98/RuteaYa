using RuteaYa.API.Servicios.Rutas;
using Xunit;

namespace RuteaYa.Tests
{
    public class RutasOpcionesTests
    {
        [Theory]
        [InlineData("Auto", "clave", true)]
        [InlineData("auto", "clave", true)]
        [InlineData("Auto", "", false)]
        [InlineData("Auto", "   ", false)]
        [InlineData("Mock", "clave", false)]
        [InlineData("Mock", "", false)]
        [InlineData("Google", "clave", true)]
        [InlineData("google", "clave", true)]
        [InlineData("Otro", "clave", false)]
        public void UsarGoogle_SegunProveedorYKey(string proveedor, string key, bool esperado)
        {
            var opciones = new RutasOpciones { Proveedor = proveedor, GoogleApiKey = key };

            Assert.Equal(esperado, opciones.UsarGoogle());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UsarGoogle_ProveedorGoogleSinKey_LanzaInvalidOperationException(string key)
        {
            var opciones = new RutasOpciones { Proveedor = "Google", GoogleApiKey = key };

            var error = Assert.Throws<InvalidOperationException>(() => opciones.UsarGoogle());

            Assert.Contains("Rutas:GoogleApiKey", error.Message);
        }

        [Fact]
        public void ValoresPorDefecto_SinConfiguracion_UsanMockConRespaldoActivado()
        {
            var opciones = new RutasOpciones();

            Assert.Equal("Auto", opciones.Proveedor);
            Assert.True(opciones.RespaldoAlMock);
            Assert.False(opciones.UsarGoogle());
        }
    }
}