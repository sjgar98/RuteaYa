using RuteaYa.API.Servicios;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class TransporteServicioTests
    {
        private const int UsuarioId = 1;

        [Fact]
        public async Task ListarAsync_DevuelveDatosMapeadosYFlagsSegunEstado()
        {
            var repositorio = new AsignacionRepositorioFalso();
            repositorio.Asignaciones.Add(CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: EstadoEnvio.ConductorAsignado));
            repositorio.Asignaciones.Add(CrearAsignacion(id: 2, conductorId: UsuarioId, estadoEnvio: EstadoEnvio.Retirado));
            var servicio = new TransporteServicio(repositorio);

            var transportes = await servicio.ListarAsync(UsuarioId);

            Assert.Equal(2, transportes.Count);

            var conductorAsignado = transportes.Single(t => t.AsignacionId == 1);
            Assert.Equal("Zapatillas", conductorAsignado.Descripcion);
            Assert.Equal("Plaza Italia, Palermo, CABA", conductorAsignado.OrigenDir);
            Assert.Equal("Belgrano, CABA", conductorAsignado.ViajeOrigenDir);
            Assert.Equal(7000m, conductorAsignado.Ganancia);
            Assert.True(conductorAsignado.PuedeMarcarRetirado);
            Assert.False(conductorAsignado.PuedeMarcarEnCamino);
            Assert.False(conductorAsignado.PuedeMarcarEntregado);

            var retirado = transportes.Single(t => t.AsignacionId == 2);
            Assert.False(retirado.PuedeMarcarRetirado);
            Assert.True(retirado.PuedeMarcarEnCamino);
            Assert.False(retirado.PuedeMarcarEntregado);
        }

        [Fact]
        public async Task ListarAsync_SoloDevuelveAsignacionesDelConductorAutenticado()
        {
            var repositorio = new AsignacionRepositorioFalso();
            repositorio.Asignaciones.Add(CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: EstadoEnvio.Retirado));
            repositorio.Asignaciones.Add(CrearAsignacion(id: 2, conductorId: 2, estadoEnvio: EstadoEnvio.Retirado));
            var servicio = new TransporteServicio(repositorio);

            var transportes = await servicio.ListarAsync(UsuarioId);

            var unico = Assert.Single(transportes);
            Assert.Equal(1, unico.AsignacionId);
        }

        [Fact]
        public async Task MarcarRetiradoAsync_AsignacionInexistente_DevuelveNoEncontrado()
        {
            var servicio = new TransporteServicio(new AsignacionRepositorioFalso());

            var resultado = await servicio.MarcarRetiradoAsync(UsuarioId, 99);

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado);
        }

        [Fact]
        public async Task MarcarRetiradoAsync_AsignacionDeOtroConductor_DevuelveNoEncontradoYNoLaModifica()
        {
            var repositorio = new AsignacionRepositorioFalso();
            var asignacion = CrearAsignacion(id: 1, conductorId: 2, estadoEnvio: EstadoEnvio.ConductorAsignado);
            repositorio.Asignaciones.Add(asignacion);
            var servicio = new TransporteServicio(repositorio);

            var resultado = await servicio.MarcarRetiradoAsync(UsuarioId, 1);

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado);
            Assert.Equal(EstadoEnvio.ConductorAsignado, asignacion.Envio.Estado);
            Assert.Equal(0, repositorio.CantidadGuardados);
        }

        [Fact]
        public async Task MarcarRetiradoAsync_DesdeConductorAsignado_CambiaEstadoYRegistraHistorial()
        {
            var repositorio = new AsignacionRepositorioFalso();
            var asignacion = CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: EstadoEnvio.ConductorAsignado);
            repositorio.Asignaciones.Add(asignacion);
            var servicio = new TransporteServicio(repositorio);

            var resultado = await servicio.MarcarRetiradoAsync(UsuarioId, 1);

            Assert.Equal(ResultadoOperacion.Exito, resultado);
            Assert.Equal(EstadoEnvio.Retirado, asignacion.Envio.Estado);
            Assert.Equal(1, repositorio.CantidadGuardados);
            var entrada = Assert.Single(asignacion.Envio.Historial);
            Assert.Equal(EstadoEnvio.ConductorAsignado, entrada.EstadoAnterior);
            Assert.Equal(EstadoEnvio.Retirado, entrada.EstadoNuevo);
        }

        [Theory]
        [InlineData(EstadoEnvio.BuscandoConductor)]
        [InlineData(EstadoEnvio.Retirado)]
        [InlineData(EstadoEnvio.EnCamino)]
        [InlineData(EstadoEnvio.Entregado)]
        [InlineData(EstadoEnvio.Cancelado)]
        public async Task MarcarRetiradoAsync_DesdeEstadoNoValido_DevuelveConflictoYNoLoModifica(EstadoEnvio estadoInicial)
        {
            var repositorio = new AsignacionRepositorioFalso();
            var asignacion = CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: estadoInicial);
            repositorio.Asignaciones.Add(asignacion);
            var servicio = new TransporteServicio(repositorio);

            var resultado = await servicio.MarcarRetiradoAsync(UsuarioId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado);
            Assert.Equal(estadoInicial, asignacion.Envio.Estado);
            Assert.Empty(asignacion.Envio.Historial);
            Assert.Equal(0, repositorio.CantidadGuardados);
        }

        [Fact]
        public async Task MarcarEnCaminoAsync_DesdeRetirado_CambiaEstado()
        {
            var repositorio = new AsignacionRepositorioFalso();
            var asignacion = CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: EstadoEnvio.Retirado);
            repositorio.Asignaciones.Add(asignacion);
            var servicio = new TransporteServicio(repositorio);

            var resultado = await servicio.MarcarEnCaminoAsync(UsuarioId, 1);

            Assert.Equal(ResultadoOperacion.Exito, resultado);
            Assert.Equal(EstadoEnvio.EnCamino, asignacion.Envio.Estado);
            Assert.Equal(1, repositorio.CantidadGuardados);
        }

        [Theory]
        [InlineData(EstadoEnvio.BuscandoConductor)]
        [InlineData(EstadoEnvio.ConductorAsignado)]
        [InlineData(EstadoEnvio.EnCamino)]
        [InlineData(EstadoEnvio.Entregado)]
        [InlineData(EstadoEnvio.Cancelado)]
        public async Task MarcarEnCaminoAsync_SinHaberRetiradoAntes_DevuelveConflicto(EstadoEnvio estadoInicial)
        {
            var repositorio = new AsignacionRepositorioFalso();
            var asignacion = CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: estadoInicial);
            repositorio.Asignaciones.Add(asignacion);
            var servicio = new TransporteServicio(repositorio);

            var resultado = await servicio.MarcarEnCaminoAsync(UsuarioId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado);
            Assert.Equal(estadoInicial, asignacion.Envio.Estado);
        }

        [Fact]
        public async Task MarcarEntregadoAsync_DesdeEnCamino_CambiaEstado()
        {
            var repositorio = new AsignacionRepositorioFalso();
            var asignacion = CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: EstadoEnvio.EnCamino);
            repositorio.Asignaciones.Add(asignacion);
            var servicio = new TransporteServicio(repositorio);

            var resultado = await servicio.MarcarEntregadoAsync(UsuarioId, 1);

            Assert.Equal(ResultadoOperacion.Exito, resultado);
            Assert.Equal(EstadoEnvio.Entregado, asignacion.Envio.Estado);
            Assert.Equal(1, repositorio.CantidadGuardados);
        }

        [Theory]
        [InlineData(EstadoEnvio.BuscandoConductor)]
        [InlineData(EstadoEnvio.ConductorAsignado)]
        [InlineData(EstadoEnvio.Retirado)]
        [InlineData(EstadoEnvio.Entregado)]
        [InlineData(EstadoEnvio.Cancelado)]
        public async Task MarcarEntregadoAsync_SinHaberEstadoEnCaminoAntes_DevuelveConflicto(EstadoEnvio estadoInicial)
        {
            var repositorio = new AsignacionRepositorioFalso();
            var asignacion = CrearAsignacion(id: 1, conductorId: UsuarioId, estadoEnvio: estadoInicial);
            repositorio.Asignaciones.Add(asignacion);
            var servicio = new TransporteServicio(repositorio);

            var resultado = await servicio.MarcarEntregadoAsync(UsuarioId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado);
            Assert.Equal(estadoInicial, asignacion.Envio.Estado);
        }

        private static Asignacion CrearAsignacion(int id, int conductorId, EstadoEnvio estadoEnvio)
        {
            return new Asignacion
            {
                Id = id,
                EnvioId = id * 10,
                ViajeId = id * 100,
                FechaViaje = new DateOnly(2026, 11, 2),
                DesvioKm = 3m,
                TiempoExtraMin = 9,
                Ganancia = 7000m,
                AsignadoEn = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc),
                Envio = new Envio
                {
                    Id = id * 10,
                    Descripcion = "Zapatillas",
                    OrigenDir = "Plaza Italia, Palermo, CABA",
                    DestinoDir = "Obelisco, CABA",
                    Estado = estadoEnvio
                },
                Viaje = new Viaje
                {
                    Id = id * 100,
                    UsuarioId = conductorId,
                    OrigenDir = "Belgrano, CABA",
                    DestinoDir = "Lomas de Zamora, Buenos Aires"
                }
            };
        }

        private class AsignacionRepositorioFalso : IAsignacionRepositorio
        {
            public List<Asignacion> Asignaciones { get; } = new List<Asignacion>();
            public int CantidadGuardados { get; private set; }

            public Task<List<Asignacion>> ListarPorConductorAsync(int conductorId)
            {
                var asignaciones = Asignaciones.Where(a => a.Viaje.UsuarioId == conductorId).ToList();
                return Task.FromResult(asignaciones);
            }

            public Task<Asignacion?> ObtenerPorIdAsync(int id)
            {
                return Task.FromResult(Asignaciones.FirstOrDefault(a => a.Id == id));
            }

            public Task AgregarAsync(List<Asignacion> asignaciones)
            {
                Asignaciones.AddRange(asignaciones);
                return Task.CompletedTask;
            }

            public Task GuardarCambiosAsync()
            {
                CantidadGuardados++;
                return Task.CompletedTask;
            }
        }
    }
}