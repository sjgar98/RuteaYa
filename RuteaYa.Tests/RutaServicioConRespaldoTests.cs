using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.API.Servicios.Rutas;
using RuteaYa.DTOs.Rutas;
using Xunit;

namespace RuteaYa.Tests
{
    public class RutaServicioConRespaldoTests
    {
        private static readonly CoordenadaDto Belgrano = new CoordenadaDto(-34.5627m, -58.4565m);
        private static readonly CoordenadaDto Lomas = new CoordenadaDto(-34.7609m, -58.4063m);
        private static readonly CoordenadaDto Quilmes = new CoordenadaDto(-34.7206m, -58.2546m);
        private static readonly CoordenadaDto Avellaneda = new CoordenadaDto(-34.6627m, -58.3653m);
        private static readonly CoordenadaDto[] Recorrido = { Belgrano, Lomas };

        public static IEnumerable<object[]> FallosDelProveedor => new[]
        {
            new object[] { new RutaNoDisponibleException("sin ruta") },
            new object[] { new HttpRequestException("sin red") },
            new object[] { new JsonException("respuesta rota") },
            new object[] { new TaskCanceledException("timeout") }
        };

        private static (RutaServicioConRespaldo servicio, PrincipalFalso principal, LoggerEspia logger) Crear()
        {
            var principal = new PrincipalFalso();
            var logger = new LoggerEspia();
            var servicio = new RutaServicioConRespaldo(principal, new RutaServicioMock(), new MemoryCache(new MemoryCacheOptions()), logger);
            return (servicio, principal, logger);
        }

        private static async Task<decimal> DistanciaDelMockAsync()
        {
            return (await new RutaServicioMock().CalcularRutaAsync(Recorrido)).DistanciaKm;
        }

        [Fact]
        public async Task CalcularRutaAsync_ElPrincipalAnda_DevuelveSuResultadoSinAvisos()
        {
            var (servicio, principal, logger) = Crear();
            principal.AlCalcular = () => Task.FromResult(new RutaResultadoDto { DistanciaKm = 99m });

            var ruta = await servicio.CalcularRutaAsync(Recorrido);

            Assert.Equal(99m, ruta.DistanciaKm);
            Assert.Empty(logger.Niveles);
        }

        [Theory]
        [MemberData(nameof(FallosDelProveedor))]
        public async Task CalcularRutaAsync_ElPrincipalFalla_RespondeElMockYAvisaEnElLog(Exception fallo)
        {
            var (servicio, principal, logger) = Crear();
            principal.AlCalcular = () => throw fallo;

            var ruta = await servicio.CalcularRutaAsync(Recorrido);

            Assert.Equal(await DistanciaDelMockAsync(), ruta.DistanciaKm);
            Assert.Single(logger.Niveles, LogLevel.Warning);
        }

        [Fact]
        public async Task CompararConEnvioAsync_ElPrincipalFalla_ComparaConElMock()
        {
            var (servicio, principal, _) = Crear();
            principal.AlComparar = () => throw new RutaNoDisponibleException("sin ruta");

            var detalle = await servicio.CompararConEnvioAsync(Recorrido, Quilmes, Avellaneda);

            Assert.True(detalle.DesvioKm > 0);
            Assert.True(detalle.DistanciaConEnvioKm > detalle.DistanciaHabitualKm);
        }

        [Fact]
        public async Task CalcularRutaAsync_CancelaElLlamador_NoSeTapaConElRespaldo()
        {
            var (servicio, principal, _) = Crear();
            principal.AlCalcular = () => throw new TaskCanceledException();
            using var cancelacion = new CancellationTokenSource();
            cancelacion.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(
                () => servicio.CalcularRutaAsync(Recorrido, cancelacion.Token));
        }

        [Fact]
        public async Task CalcularRutaAsync_ErrorDeProgramacion_NoSeTapaConElRespaldo()
        {
            var (servicio, principal, _) = Crear();
            principal.AlCalcular = () => throw new ArgumentException("recorrido inválido");

            await Assert.ThrowsAsync<ArgumentException>(() => servicio.CalcularRutaAsync(Recorrido));
        }

        [Fact]
        public async Task CalcularRutaAsync_FallaLaRed_LasSiguientesLlamadasNoEsperanAlPrincipal()
        {
            var (servicio, principal, logger) = Crear();
            principal.AlCalcular = () => throw new HttpRequestException("sin red");

            await servicio.CalcularRutaAsync(Recorrido);
            await servicio.CalcularRutaAsync(Recorrido);
            await servicio.CalcularRutaAsync(Recorrido);

            Assert.Equal(1, principal.Llamadas);
            Assert.Single(logger.Niveles, LogLevel.Warning);
        }

        [Fact]
        public async Task CalcularRutaAsync_RutaNoEncontrada_NoPausaAlPrincipal()
        {
           
            var (servicio, principal, _) = Crear();
            principal.AlCalcular = () => throw new RutaNoDisponibleException("sin ruta");

            await servicio.CalcularRutaAsync(Recorrido);
            await servicio.CalcularRutaAsync(Recorrido);

            Assert.Equal(2, principal.Llamadas);
        }

        
        private sealed class PrincipalFalso : IRutaServicio
        {
            public int Llamadas { get; private set; }
            public Func<Task<RutaResultadoDto>>? AlCalcular { get; set; }
            public Func<Task<DetalleOportunidadRutaDto>>? AlComparar { get; set; }

            public Task<RutaResultadoDto> CalcularRutaAsync(IReadOnlyList<CoordenadaDto> recorrido, CancellationToken cancelacion = default)
            {
                Llamadas++;
                return AlCalcular!();
            }

            public Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(
                IReadOnlyList<CoordenadaDto> recorridoHabitual, CoordenadaDto retiro, CoordenadaDto entrega, CancellationToken cancelacion = default)
            {
                Llamadas++;
                return AlComparar!();
            }

            public Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion) => throw new NotImplementedException();
            public Task<RutaResultadoDto> CalcularRutaConParadasAsync(List<string> paradas) => throw new NotImplementedException();
            public Task<RutaResultadoDto> CalcularRutaAlternativaEnvioAsync(string origen, string destino, string retiroPaquete, string entregaPaquete) => throw new NotImplementedException();
            public Task<decimal> CalcularDesvioAsync(string rutaOriginalId, string paradaExtra) => throw new NotImplementedException();
            public Task<DetalleOportunidadRutaDto> ObtenerDetalleOportunidadAsync(string origenHabitual, string destinoHabitual, string retiroEnvio, string entregaEnvio) => throw new NotImplementedException();
            public Task<List<string>> BuscarDireccionesAsync(string texto) => throw new NotImplementedException();
        }

        
        private sealed class LoggerEspia : ILogger<RutaServicioConRespaldo>
        {
            public List<LogLevel> Niveles { get; } = new List<LogLevel>();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                
                if (logLevel >= LogLevel.Warning)
                {
                    Niveles.Add(logLevel);
                }
            }
        }
    }
}