using RuteaYa.API.Servicios;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class HistorialServicioTests
    {
        private const int UsuarioId = 1;

        private readonly AsignacionRepositorioFalso _asignaciones = new AsignacionRepositorioFalso();
        private readonly ViajeRepositorioFalso _viajes = new ViajeRepositorioFalso();

        private HistorialServicio CrearServicio() => new HistorialServicio(_asignaciones, _viajes);

        [Fact]
        public async Task ObtenerHistorialAsync_SinAsignaciones_DevuelveMetricasEnCeroEItemsVacios()
        {
            var servicio = CrearServicio();

            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            Assert.NotNull(resultado);
            Assert.Equal(0, resultado.ViajesRealizados);
            Assert.Equal(0, resultado.EnviosTransportados);
            Assert.Equal(0m, resultado.GananciaEfectiva);
            Assert.Empty(resultado.Items);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_ConAsignacionesEntregadas_CalculaMetricasYAgrupaCorrectamente()
        {
            var viaje = new Viaje
            {
                Id = 10,
                UsuarioId = UsuarioId,
                OrigenDir = "Palermo, CABA",
                DestinoDir = "Obelisco, CABA",
                Activo = true
            };
            _viajes.Viajes.Add(viaje);

            var envio = new Envio
            {
                Id = 100,
                UsuarioId = 2,
                Estado = EstadoEnvio.Entregado,
                Monto = 5000m
            };

            var asignacion = new Asignacion
            {
                Id = 1,
                ViajeId = 10,
                EnvioId = 100,
                AsignadoEn = new DateTime(2026, 11, 2, 10, 0, 0, DateTimeKind.Utc),
                Ganancia = 5000m,
                Viaje = viaje,
                Envio = envio
            };
            _asignaciones.Asignaciones.Add(asignacion);

            var servicio = CrearServicio();
            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.ViajesRealizados);
            Assert.Equal(1, resultado.EnviosTransportados);
            Assert.Equal(5000m, resultado.GananciaEfectiva);

            var item = Assert.Single(resultado.Items);
            Assert.Equal(10, item.Id);
            Assert.Equal("Viaje", item.Tipo);
            Assert.Equal("Palermo, CABA → Obelisco, CABA", item.Descripcion);
            Assert.Equal(5000m, item.Monto);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_AplicaFiltroDeFechasCorrectamente()
        {
            var viaje = new Viaje { Id = 1, UsuarioId = UsuarioId, OrigenDir = "A", DestinoDir = "B", Activo = true };
            var envio1 = new Envio { Id = 10, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 3000m };
            var envio2 = new Envio { Id = 11, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 4000m };

            _viajes.Viajes.Add(viaje);
            
            _asignaciones.Asignaciones.Add(new Asignacion
            {
                Id = 1,
                ViajeId = 1,
                EnvioId = 10,
                AsignadoEn = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 3000m,
                Viaje = viaje,
                Envio = envio1
            });
            _asignaciones.Asignaciones.Add(new Asignacion
            {
                Id = 2,
                ViajeId = 1,
                EnvioId = 11,
                AsignadoEn = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 4000m,
                Viaje = viaje,
                Envio = envio2
            });

            var servicio = CrearServicio();

            // Filtrar desde el 1 de noviembre de 2026 en adelante
            var desde = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, desde, null, null, null);

            Assert.Equal(1, resultado.EnviosTransportados);
            Assert.Equal(4000m, resultado.GananciaEfectiva);
        }

        [Theory]
        [InlineData("fecha_asc")]
        [InlineData("fecha_desc")]
        [InlineData("monto_asc")]
        [InlineData("monto_desc")]
        public async Task ObtenerHistorialAsync_AplicaOrdenamientosCorrectamente(string criterioOrden)
        {
            var viaje1 = new Viaje { Id = 1, UsuarioId = UsuarioId, OrigenDir = "A", DestinoDir = "B", Activo = true };
            var viaje2 = new Viaje { Id = 2, UsuarioId = UsuarioId, OrigenDir = "C", DestinoDir = "D", Activo = true };
            var envio1 = new Envio { Id = 10, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 1000m };
            var envio2 = new Envio { Id = 11, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 5000m };

            _viajes.Viajes.AddRange(new[] { viaje1, viaje2 });
            
            _asignaciones.Asignaciones.Add(new Asignacion
            {
                Id = 1,
                ViajeId = 1,
                EnvioId = 10,
                AsignadoEn = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 1000m,
                Viaje = viaje1,
                Envio = envio1
            });
            _asignaciones.Asignaciones.Add(new Asignacion
            {
                Id = 2,
                ViajeId = 2,
                EnvioId = 11,
                AsignadoEn = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 5000m,
                Viaje = viaje2,
                Envio = envio2
            });

            var servicio = CrearServicio();
            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, criterioOrden, null);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Items.Count);
        }

        private class AsignacionRepositorioFalso : IAsignacionRepositorio
        {
            public List<Asignacion> Asignaciones { get; } = new List<Asignacion>();

            public Task<List<Asignacion>> ListarEntregadasPorConductorAsync(int conductorId, DateTime? desde, DateTime? hasta)
            {
                return Task.FromResult(Asignaciones
                    .Where(a => a.Viaje.UsuarioId == conductorId && a.Envio.Estado == EstadoEnvio.Entregado)
                    .Where(a => !desde.HasValue || a.AsignadoEn >= desde.Value)
                    .Where(a => !hasta.HasValue || a.AsignadoEn <= hasta.Value)
                    .ToList());
            }

            public Task<List<Asignacion>> ListarPorConductorAsync(int conductorId) => throw new NotImplementedException();
            public Task<Asignacion?> ObtenerPorIdAsync(int id) => throw new NotImplementedException();
            public Task AgregarAsync(List<Asignacion> asignaciones) => throw new NotImplementedException();
            public Task GuardarCambiosAsync() => throw new NotImplementedException();
        }

        private class ViajeRepositorioFalso : IViajeRepositorio
        {
            public List<Viaje> Viajes { get; } = new List<Viaje>();

            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null)
                => Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId && (activo == null || v.Activo == activo)).ToList());

            public Task<Viaje?> ObtenerPorIdAsync(int id) => throw new NotImplementedException();
            public Task AgregarAsync(Viaje viaje) => throw new NotImplementedException();
            public Task GuardarCambiosAsync() => throw new NotImplementedException();
        }
    }
}
