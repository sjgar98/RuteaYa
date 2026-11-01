using RuteaYa.API.Servicios.Direcciones;
using Xunit;

namespace RuteaYa.Tests
{
    public class DireccionServicioFijoTests
    {
        [Theory]
        [InlineData("moron")]
        [InlineData("MORÓN")]
        [InlineData("  morón  ")]
        public async Task BuscarAsync_IgnoraTildesMayusculasYEspacios(string texto)
        {
            var direcciones = await new DireccionServicioFijo().BuscarAsync(texto);

            Assert.Contains(direcciones, d => d.Direccion == "Morón, Buenos Aires");
        }

        [Fact]
        public async Task BuscarAsync_SinCoincidencias_DevuelveListaVacia()
        {
            var direcciones = await new DireccionServicioFijo().BuscarAsync("zzzz");

            Assert.Empty(direcciones);
        }
    }
}