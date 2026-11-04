using RuteaYa.API.Servicios;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class HistorialServicioTests
    {
        private const int UsuarioId = 1;
        private const int OtroUsuarioId = 2;

        private readonly AsignacionRepositorioFalso _asignaciones = new AsignacionRepositorioFalso();
        private readonly EnvioRepositorioFalso _envios = new EnvioRepositorioFalso();

        private readonly Viaje _belgranoLomas = CrearViaje(1, "Belgrano, CABA", "Lomas de Zamora, Buenos Aires");
        private readonly Viaje _palermoSanIsidro = CrearViaje(2, "Palermo, CABA", "San Isidro, Buenos Aires");

        private HistorialServicio CrearServicio() => new HistorialServicio(_asignaciones, _envios);

        [Fact]
        public async Task ObtenerHistorialAsync_SinOperaciones_DevuelveContadoresEnCeroYSinItems()
        {
            var resultado = await CrearServicio().ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            Assert.Equal(0, resultado.ViajesRealizados);
            Assert.Equal(0, resultado.EnviosTransportados);
            Assert.Equal(0m, resultado.GananciaEfectiva);
            Assert.Empty(resultado.Items);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_CuentaSalidasDistintasEnviosYGananciaEfectiva()
        {
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 7), 4000m);
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 7), 5500m);
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 9), 2500m);
            AgregarEntregada(_palermoSanIsidro, new DateOnly(2026, 9, 11), 3000m);

            var resultado = await CrearServicio().ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            Assert.Equal(3, resultado.ViajesRealizados);
            Assert.Equal(4, resultado.EnviosTransportados);
            Assert.Equal(15000m, resultado.GananciaEfectiva);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_AgrupaLosViajesPorMesConElMesEnEspanol()
        {
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 7), 4000m);
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 21), 5500m);
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 10, 5), 2500m);

            var resultado = await CrearServicio().ObtenerHistorialAsync(UsuarioId, null, null, "fecha_asc", null);

            Assert.Equal(2, resultado.Items.Count);
            var septiembre = resultado.Items[0];
            Assert.Equal(HistorialServicio.TipoViaje, septiembre.Tipo);
            Assert.Equal(1, septiembre.Id);
            Assert.Equal("Belgrano, CABA → Lomas de Zamora, Buenos Aires", septiembre.Descripcion);
            Assert.Equal("Septiembre · 2 paquetes", septiembre.Detalle);
            Assert.Equal(9500m, septiembre.Monto);
            Assert.Equal(new DateOnly(2026, 9, 21), septiembre.Fecha);
            Assert.Null(septiembre.Estado);
            Assert.Equal("Octubre · 1 paquete", resultado.Items[1].Detalle);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_IncluyeLosEnviosPropiosFinalizadosConLaFechaDelCierre()
        {
            _envios.Envios.Add(CrearEnvioPropio(20, EstadoEnvio.Entregado, new DateTime(2026, 9, 30, 1, 0, 0, DateTimeKind.Utc)));
            _envios.Envios.Add(CrearEnvioPropio(21, EstadoEnvio.BuscandoConductor, new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc)));

            var resultado = await CrearServicio().ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            var item = Assert.Single(resultado.Items);
            Assert.Equal(HistorialServicio.TipoEnvio, item.Tipo);
            Assert.Equal(20, item.Id);
            Assert.Equal("Avellaneda, Buenos Aires → Palermo, CABA", item.Descripcion);
            Assert.Equal("Repuesto", item.Detalle);
            Assert.Equal(EstadoEnvio.Entregado, item.Estado);
            Assert.Equal(new DateOnly(2026, 9, 29), item.Fecha);
            Assert.Equal(0, resultado.EnviosTransportados);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_NoMuestraOperacionesDeOtrosUsuarios()
        {
            var viajeAjeno = CrearViaje(3, "Morón, Buenos Aires", "Caballito, CABA", OtroUsuarioId);
            AgregarEntregada(viajeAjeno, new DateOnly(2026, 9, 7), 4000m);
            var envioAjeno = CrearEnvioPropio(30, EstadoEnvio.Entregado, new DateTime(2026, 9, 30, 15, 0, 0, DateTimeKind.Utc));
            envioAjeno.UsuarioId = OtroUsuarioId;
            _envios.Envios.Add(envioAjeno);

            var resultado = await CrearServicio().ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            Assert.Equal(0, resultado.EnviosTransportados);
            Assert.Empty(resultado.Items);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_FiltroDeFechasIncluyeDesdeYHasta()
        {
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 6), 1000m);
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 7), 2000m);
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 30), 3000m);
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 10, 1), 4000m);
            _envios.Envios.Add(CrearEnvioPropio(20, EstadoEnvio.Entregado, new DateTime(2026, 9, 15, 15, 0, 0, DateTimeKind.Utc)));
            _envios.Envios.Add(CrearEnvioPropio(21, EstadoEnvio.Cancelado, new DateTime(2026, 10, 15, 15, 0, 0, DateTimeKind.Utc)));

            var resultado = await CrearServicio().ObtenerHistorialAsync(
                UsuarioId, new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 30), null, null);

            Assert.Equal(2, resultado.EnviosTransportados);
            Assert.Equal(5000m, resultado.GananciaEfectiva);
            Assert.Equal(new[] { 20, 1 }, resultado.Items.OrderBy(i => i.Tipo).Select(i => i.Id));
        }

        [Theory]
        [InlineData("viajes", new[] { 1 })]
        [InlineData("entregados", new[] { 20 })]
        [InlineData("cancelados", new[] { 21 })]
        [InlineData("todos", new[] { 1, 20, 21 })]
        [InlineData(null, new[] { 1, 20, 21 })]
        public async Task ObtenerHistorialAsync_FiltraPorEstado(string? estado, int[] idsEsperados)
        {
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 7), 4000m);
            _envios.Envios.Add(CrearEnvioPropio(20, EstadoEnvio.Entregado, new DateTime(2026, 9, 15, 15, 0, 0, DateTimeKind.Utc)));
            _envios.Envios.Add(CrearEnvioPropio(21, EstadoEnvio.Cancelado, new DateTime(2026, 9, 16, 15, 0, 0, DateTimeKind.Utc)));

            var resultado = await CrearServicio().ObtenerHistorialAsync(UsuarioId, null, null, null, estado);

            Assert.Equal(idsEsperados, resultado.Items.Select(i => i.Id).OrderBy(id => id));
            Assert.Equal(1, resultado.EnviosTransportados);
        }

        [Theory]
        [InlineData("fecha_desc", new[] { 2, 1 })]
        [InlineData(null, new[] { 2, 1 })]
        [InlineData("fecha_asc", new[] { 1, 2 })]
        [InlineData("monto_desc", new[] { 1, 2 })]
        [InlineData("monto_asc", new[] { 2, 1 })]
        public async Task ObtenerHistorialAsync_OrdenaPorFechaOMonto(string? orden, int[] idsEsperados)
        {
            AgregarEntregada(_belgranoLomas, new DateOnly(2026, 9, 7), 9000m);
            AgregarEntregada(_palermoSanIsidro, new DateOnly(2026, 9, 11), 3000m);

            var resultado = await CrearServicio().ObtenerHistorialAsync(UsuarioId, null, null, orden, null);

            Assert.Equal(idsEsperados, resultado.Items.Select(i => i.Id));
        }

        private void AgregarEntregada(Viaje viaje, DateOnly fechaViaje, decimal ganancia)
        {
            var id = _asignaciones.Asignaciones.Count + 1;
            var envio = new Envio { Id = 100 + id, UsuarioId = OtroUsuarioId, Estado = EstadoEnvio.Entregado, Monto = ganancia };
            _asignaciones.Asignaciones.Add(new Asignacion
            {
                Id = id,
                ViajeId = viaje.Id,
                EnvioId = envio.Id,
                FechaViaje = fechaViaje,
                Ganancia = ganancia,
                Viaje = viaje,
                Envio = envio
            });
        }

        private static Viaje CrearViaje(int id, string origen, string destino, int usuarioId = UsuarioId)
        {
            return new Viaje { Id = id, UsuarioId = usuarioId, OrigenDir = origen, DestinoDir = destino, Activo = true };
        }

        private static Envio CrearEnvioPropio(int id, EstadoEnvio estado, DateTime cierreUtc)
        {
            return new Envio
            {
                Id = id,
                UsuarioId = UsuarioId,
                OrigenDir = "Avellaneda, Buenos Aires",
                DestinoDir = "Palermo, CABA",
                Descripcion = "Repuesto",
                Monto = 2500m,
                Estado = estado,
                CreadoEn = cierreUtc.AddDays(-5),
                Historial = new List<EnvioEstadoHistorial>
                {
                    new EnvioEstadoHistorial { EstadoNuevo = EstadoEnvio.BuscandoConductor, Fecha = cierreUtc.AddDays(-5) },
                    new EnvioEstadoHistorial { EstadoAnterior = EstadoEnvio.BuscandoConductor, EstadoNuevo = estado, Fecha = cierreUtc }
                }
            };
        }

        private class AsignacionRepositorioFalso : IAsignacionRepositorio
        {
            public List<Asignacion> Asignaciones { get; } = new List<Asignacion>();

            public Task<List<Asignacion>> ListarEntregadasPorConductorAsync(int conductorId, DateOnly? desde, DateOnly? hasta)
            {
                return Task.FromResult(Asignaciones
                    .Where(a => a.Viaje.UsuarioId == conductorId && a.Envio.Estado == EstadoEnvio.Entregado)
                    .Where(a => !desde.HasValue || a.FechaViaje >= desde.Value)
                    .Where(a => !hasta.HasValue || a.FechaViaje <= hasta.Value)
                    .ToList());
            }

            public Task<List<Asignacion>> ListarPorConductorAsync(int conductorId) => throw new NotImplementedException();
            public Task<Asignacion?> ObtenerPorIdAsync(int id) => throw new NotImplementedException();
            public Task AgregarAsync(List<Asignacion> asignaciones) => throw new NotImplementedException();
            public Task GuardarCambiosAsync() => throw new NotImplementedException();
        }

        private class EnvioRepositorioFalso : IEnvioRepositorio
        {
            public List<Envio> Envios { get; } = new List<Envio>();

            public Task<List<Envio>> ListarFinalizadosPorUsuarioAsync(int usuarioId)
            {
                return Task.FromResult(Envios
                    .Where(e => e.UsuarioId == usuarioId && (e.Estado == EstadoEnvio.Entregado || e.Estado == EstadoEnvio.Cancelado))
                    .ToList());
            }

            public Task<List<Envio>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null) => throw new NotImplementedException();
            public Task<List<Envio>> ListarDisponiblesAsync(DateOnly desde, int usuarioIdExcluido) => throw new NotImplementedException();
            public Task<List<Envio>> ListarPorIdsAsync(List<int> ids) => throw new NotImplementedException();
            public Task<Envio?> ObtenerPorIdAsync(int id) => throw new NotImplementedException();
            public Task AgregarAsync(Envio envio) => throw new NotImplementedException();
            public Task GuardarCambiosAsync() => throw new NotImplementedException();
        }
    }
}
