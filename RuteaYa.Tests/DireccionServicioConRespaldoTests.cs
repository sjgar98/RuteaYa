using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RuteaYa.API.Servicios.Direcciones;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using Xunit;

namespace RuteaYa.Tests
{
    public class DireccionServicioConRespaldoTests
    {
        public static IEnumerable<object[]> FallosDelProveedor => new[]
        {
            new object[] { new DireccionesNoDisponiblesException("throttled") },
            new object[] { new HttpRequestException("sin red") },
            new object[] { new TaskCanceledException("timeout") }
        };

        private static (DireccionServicioConRespaldo servicio, PrincipalFalso principal, LoggerEspia<DireccionServicioConRespaldo> logger) Crear()
        {
            var principal = new PrincipalFalso();
            var logger = new LoggerEspia<DireccionServicioConRespaldo>();
            var servicio = new DireccionServicioConRespaldo(
                principal, new DireccionServicioFijo(), new MemoryCache(new MemoryCacheOptions()), logger);
            return (servicio, principal, logger);
        }

        [Fact]
        public async Task BuscarAsync_ElPrincipalAnda_DevuelveSuResultadoSinAvisos()
        {
            var (servicio, principal, logger) = Crear();
            principal.AlBuscar = () => Task.FromResult(new List<DireccionDto> { new DireccionDto { Direccion = "Calle Falsa 123" } });

            var direcciones = await servicio.BuscarAsync("calle");

            Assert.Equal("Calle Falsa 123", direcciones.Single().Direccion);
            Assert.Empty(logger.Niveles);
        }

        [Theory]
        [MemberData(nameof(FallosDelProveedor))]
        public async Task BuscarAsync_ElPrincipalFalla_RespondeLaListaFijaYAvisaEnElLog(Exception fallo)
        {
            var (servicio, principal, logger) = Crear();
            principal.AlBuscar = () => throw fallo;

            var direcciones = await servicio.BuscarAsync("moron");

            Assert.Contains(direcciones, d => d.Direccion == "Morón, Buenos Aires");
            Assert.Single(logger.Niveles, LogLevel.Warning);
        }

        [Fact]
        public async Task BuscarAsync_TrasUnFallo_LasSiguientesBusquedasNoEsperanAlPrincipal()
        {
            var (servicio, principal, logger) = Crear();
            principal.AlBuscar = () => throw new DireccionesNoDisponiblesException("throttled");

            await servicio.BuscarAsync("moron");
            await servicio.BuscarAsync("quilmes");
            await servicio.BuscarAsync("lomas");

            Assert.Equal(1, principal.Llamadas);
            Assert.Single(logger.Niveles, LogLevel.Warning);
        }

        [Fact]
        public async Task BuscarAsync_CancelaElLlamador_NoSeTapaConElRespaldo()
        {
            var (servicio, principal, _) = Crear();
            principal.AlBuscar = () => throw new TaskCanceledException();
            using var cancelacion = new CancellationTokenSource();
            cancelacion.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(() => servicio.BuscarAsync("moron", cancelacion.Token));
        }

        [Fact]
        public async Task BuscarAsync_ErrorDeProgramacion_NoSeTapaConElRespaldo()
        {
            var (servicio, principal, _) = Crear();
            principal.AlBuscar = () => throw new ArgumentException("texto inválido");

            await Assert.ThrowsAsync<ArgumentException>(() => servicio.BuscarAsync("moron"));
        }

        private sealed class PrincipalFalso : IDireccionServicio
        {
            public int Llamadas { get; private set; }
            public Func<Task<List<DireccionDto>>>? AlBuscar { get; set; }

            public Task<List<DireccionDto>> BuscarAsync(string texto, CancellationToken cancelacion = default)
            {
                Llamadas++;
                return AlBuscar!();
            }
        }
    }
}