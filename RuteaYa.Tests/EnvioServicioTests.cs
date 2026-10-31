using RuteaYa.API.Servicios;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class EnvioServicioTests
    {
        private const int UsuarioId = 1;

        [Fact]
        public async Task ListarPorUsuarioAsync_DevuelveDatosYPuedeCancelarSegunEstado()
        {
            var repositorio = new EnvioRepositorioFalso();
            repositorio.Envios.Add(CrearEnvio(id: 1, usuarioId: UsuarioId, estado: EstadoEnvio.BuscandoConductor));
            repositorio.Envios.Add(CrearEnvio(id: 2, usuarioId: UsuarioId, estado: EstadoEnvio.Entregado));
            var servicio = new EnvioServicio(repositorio);

            var envios = await servicio.ListarPorUsuarioAsync(UsuarioId, null);

            Assert.Equal(2, envios.Count);
            var buscandoConductor = envios.Single(e => e.Id == 1);
            Assert.Equal("Plaza Italia, Palermo, CABA", buscandoConductor.OrigenDir);
            Assert.Equal("Obelisco, CABA", buscandoConductor.DestinoDir);
            Assert.Equal(EstadoEnvio.BuscandoConductor, buscandoConductor.Estado);
            Assert.True(buscandoConductor.PuedeCancelar);

            var entregado = envios.Single(e => e.Id == 2);
            Assert.False(entregado.PuedeCancelar);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_EnvioInexistente_DevuelveNull()
        {
            var servicio = new EnvioServicio(new EnvioRepositorioFalso());

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 99);

            Assert.Null(detalle);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_EnvioDeOtroUsuario_DevuelveNull()
        {
            var repositorio = new EnvioRepositorioFalso();
            repositorio.Envios.Add(CrearEnvio(id: 5, usuarioId: 2));
            var servicio = new EnvioServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.Null(detalle);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_EnvioPropioSinAsignacion_DevuelveDatosYViajeAsociadoNulo()
        {
            var repositorio = new EnvioRepositorioFalso();
            repositorio.Envios.Add(CrearEnvio(id: 5, usuarioId: UsuarioId));
            var servicio = new EnvioServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.NotNull(detalle);
            Assert.Equal(5, detalle.Id);
            Assert.Equal("Plaza Italia, Palermo, CABA", detalle.OrigenDir);
            Assert.Equal(-34.5810m, detalle.OrigenLat);
            Assert.Equal("Obelisco, CABA", detalle.DestinoDir);
            Assert.Equal(TamanoEnvio.Pequeno, detalle.Tamano);
            Assert.Equal(4000m, detalle.Monto);
            Assert.Null(detalle.ViajeAsociado);
            Assert.Empty(detalle.Historial);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_EnvioConAsignacion_DevuelveDatosDelViajeAsociado()
        {
            var repositorio = new EnvioRepositorioFalso();
            var envio = CrearEnvio(id: 5, usuarioId: UsuarioId, estado: EstadoEnvio.ConductorAsignado);
            envio.Asignacion = new Asignacion
            {
                ViajeId = 10,
                FechaViaje = new DateOnly(2026, 11, 2),
                DesvioKm = 3m,
                Ganancia = 7000m,
                Viaje = new Viaje { OrigenDir = "Belgrano, CABA", DestinoDir = "Lomas de Zamora, Buenos Aires" }
            };
            repositorio.Envios.Add(envio);
            var servicio = new EnvioServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.NotNull(detalle);
            Assert.NotNull(detalle.ViajeAsociado);
            Assert.Equal(10, detalle.ViajeAsociado.ViajeId);
            Assert.Equal("Belgrano, CABA", detalle.ViajeAsociado.OrigenDir);
            Assert.Equal("Lomas de Zamora, Buenos Aires", detalle.ViajeAsociado.DestinoDir);
            Assert.Equal(new DateOnly(2026, 11, 2), detalle.ViajeAsociado.FechaViaje);
            Assert.Equal(3m, detalle.ViajeAsociado.DesvioKm);
            Assert.Equal(7000m, detalle.ViajeAsociado.Ganancia);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_DevuelveHistorialMapeado()
        {
            var repositorio = new EnvioRepositorioFalso();
            var envio = CrearEnvio(id: 5, usuarioId: UsuarioId, estado: EstadoEnvio.ConductorAsignado);
            var fecha1 = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);
            var fecha2 = new DateTime(2026, 10, 21, 9, 0, 0, DateTimeKind.Utc);
            envio.Historial.Add(new EnvioEstadoHistorial { EstadoAnterior = null, EstadoNuevo = EstadoEnvio.BuscandoConductor, Fecha = fecha1 });
            envio.Historial.Add(new EnvioEstadoHistorial { EstadoAnterior = EstadoEnvio.BuscandoConductor, EstadoNuevo = EstadoEnvio.ConductorAsignado, Fecha = fecha2 });
            repositorio.Envios.Add(envio);
            var servicio = new EnvioServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.NotNull(detalle);
            Assert.Equal(2, detalle.Historial.Count);
            Assert.Null(detalle.Historial[0].EstadoAnterior);
            Assert.Equal(EstadoEnvio.BuscandoConductor, detalle.Historial[0].EstadoNuevo);
            Assert.Equal(EstadoEnvio.BuscandoConductor, detalle.Historial[1].EstadoAnterior);
            Assert.Equal(EstadoEnvio.ConductorAsignado, detalle.Historial[1].EstadoNuevo);
        }

        [Fact]
        public async Task CancelarAsync_EnvioInexistente_DevuelveNoEncontrado()
        {
            var servicio = new EnvioServicio(new EnvioRepositorioFalso());

            var resultado = await servicio.CancelarAsync(UsuarioId, 99);

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado);
        }

        [Fact]
        public async Task CancelarAsync_EnvioDeOtroUsuario_DevuelveNoEncontradoYNoLoModifica()
        {
            var repositorio = new EnvioRepositorioFalso();
            var envio = CrearEnvio(id: 5, usuarioId: 2, estado: EstadoEnvio.BuscandoConductor);
            repositorio.Envios.Add(envio);
            var servicio = new EnvioServicio(repositorio);

            var resultado = await servicio.CancelarAsync(UsuarioId, 5);

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado);
            Assert.Equal(EstadoEnvio.BuscandoConductor, envio.Estado);
            Assert.Equal(0, repositorio.CantidadGuardados);
        }

        [Theory]
        [InlineData(EstadoEnvio.BuscandoConductor)]
        [InlineData(EstadoEnvio.ConductorAsignado)]
        public async Task CancelarAsync_DesdeEstadoCancelable_CancelaYRegistraHistorial(EstadoEnvio estadoInicial)
        {
            var repositorio = new EnvioRepositorioFalso();
            var envio = CrearEnvio(id: 5, usuarioId: UsuarioId, estado: estadoInicial);
            repositorio.Envios.Add(envio);
            var servicio = new EnvioServicio(repositorio);

            var resultado = await servicio.CancelarAsync(UsuarioId, 5);

            Assert.Equal(ResultadoOperacion.Exito, resultado);
            Assert.Equal(EstadoEnvio.Cancelado, envio.Estado);
            Assert.Equal(1, repositorio.CantidadGuardados);
            var entrada = Assert.Single(envio.Historial);
            Assert.Equal(estadoInicial, entrada.EstadoAnterior);
            Assert.Equal(EstadoEnvio.Cancelado, entrada.EstadoNuevo);
        }

        [Theory]
        [InlineData(EstadoEnvio.Retirado)]
        [InlineData(EstadoEnvio.EnCamino)]
        [InlineData(EstadoEnvio.Entregado)]
        [InlineData(EstadoEnvio.Cancelado)]
        public async Task CancelarAsync_DesdeEstadoNoCancelable_DevuelveConflictoYNoLoModifica(EstadoEnvio estadoInicial)
        {
            var repositorio = new EnvioRepositorioFalso();
            var envio = CrearEnvio(id: 5, usuarioId: UsuarioId, estado: estadoInicial);
            repositorio.Envios.Add(envio);
            var servicio = new EnvioServicio(repositorio);

            var resultado = await servicio.CancelarAsync(UsuarioId, 5);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado);
            Assert.Equal(estadoInicial, envio.Estado);
            Assert.Empty(envio.Historial);
            Assert.Equal(0, repositorio.CantidadGuardados);
        }

        [Fact]
        public async Task CrearAsync_GuardaEnvioConDatosYEstadoInicialEnHistorial()
        {
            var repositorio = new EnvioRepositorioFalso();
            var servicio = new EnvioServicio(repositorio);

            var creado = await servicio.CrearAsync(UsuarioId, CrearDtoNuevo());

            Assert.Equal("Plaza Italia, Palermo, CABA", creado.OrigenDir);
            Assert.Equal("Obelisco, CABA", creado.DestinoDir);
            Assert.Equal(EstadoEnvio.BuscandoConductor, creado.Estado);
            Assert.True(creado.PuedeCancelar);

            var envio = Assert.Single(repositorio.Envios);
            Assert.Equal(UsuarioId, envio.UsuarioId);
            Assert.Equal("Zapatillas", envio.Descripcion);
            Assert.Equal(-34.5810m, envio.OrigenLat);
            Assert.Equal(4000m, envio.Monto);
            Assert.Equal(TamanoEnvio.Pequeno, envio.Tamano);
            Assert.Equal(1, repositorio.CantidadAgregados);

            var entrada = Assert.Single(envio.Historial);
            Assert.Null(entrada.EstadoAnterior);
            Assert.Equal(EstadoEnvio.BuscandoConductor, entrada.EstadoNuevo);
            Assert.Equal(envio.CreadoEn, entrada.Fecha);
        }

        private static CrearEnvioDto CrearDtoNuevo()
        {
            return new CrearEnvioDto
            {
                OrigenDir = " Plaza Italia, Palermo, CABA ",
                OrigenLat = -34.5810m,
                OrigenLng = -58.4208m,
                DestinoDir = "Obelisco, CABA",
                DestinoLat = -34.6037m,
                DestinoLng = -58.3816m,
                FechaLimite = new DateOnly(2026, 11, 20),
                PesoKg = 1m,
                Tamano = TamanoEnvio.Pequeno,
                Descripcion = " Zapatillas ",
                Monto = 4000m
            };
        }

        private static Envio CrearEnvio(int id, int usuarioId, EstadoEnvio estado = EstadoEnvio.BuscandoConductor)
        {
            return new Envio
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = "Plaza Italia, Palermo, CABA",
                OrigenLat = -34.5810m,
                OrigenLng = -58.4208m,
                DestinoDir = "Obelisco, CABA",
                DestinoLat = -34.6037m,
                DestinoLng = -58.3816m,
                FechaLimite = new DateOnly(2026, 11, 20),
                PesoKg = 1m,
                Tamano = TamanoEnvio.Pequeno,
                Descripcion = "Zapatillas",
                Monto = 4000m,
                Estado = estado,
                CreadoEn = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }

        private class EnvioRepositorioFalso : IEnvioRepositorio
        {
            public List<Envio> Envios { get; } = new List<Envio>();
            public int CantidadGuardados { get; private set; }
            public int CantidadAgregados { get; private set; }

            public Task<List<Envio>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null)
            {
                var envios = Envios
                    .Where(e => e.UsuarioId == usuarioId && (!estado.HasValue || e.Estado == estado.Value))
                    .ToList();
                return Task.FromResult(envios);
            }

            public Task<List<Envio>> ListarDisponiblesAsync(DateOnly desde, int usuarioIdExcluido)
            {
                var envios = Envios
                    .Where(e => e.Estado == EstadoEnvio.BuscandoConductor
                             && e.FechaLimite >= desde
                             && e.UsuarioId != usuarioIdExcluido)
                    .ToList();
                return Task.FromResult(envios);
            }

            public Task<List<Envio>> ListarPorIdsAsync(List<int> ids)
            {
                return Task.FromResult(Envios.Where(e => ids.Contains(e.Id)).ToList());
            }

            public Task<Envio?> ObtenerPorIdAsync(int id)
            {
                return Task.FromResult(Envios.FirstOrDefault(e => e.Id == id));
            }

            public Task AgregarAsync(Envio envio)
            {
                Envios.Add(envio);
                CantidadAgregados++;
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