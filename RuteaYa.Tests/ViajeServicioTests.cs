using RuteaYa.API.Servicios;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class ViajeServicioTests
    {
        private const int UsuarioId = 1;

        [Fact]
        public async Task ObtenerDetalleAsync_ViajeInexistente_DevuelveNull()
        {
            var servicio = new ViajeServicio(new ViajeRepositorioFalso());

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 99);

            Assert.Null(detalle);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_ViajeDeOtroUsuario_DevuelveNull()
        {
            var repositorio = new ViajeRepositorioFalso();
            repositorio.Viajes.Add(CrearViaje(id: 5, usuarioId: 2));
            var servicio = new ViajeServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.Null(detalle);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_ViajePropio_DevuelveDatosParadasEnOrdenYPaquetes()
        {
            var repositorio = new ViajeRepositorioFalso();
            repositorio.Viajes.Add(CrearViaje(id: 5, usuarioId: UsuarioId));
            var servicio = new ViajeServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.NotNull(detalle);
            Assert.Equal(5, detalle.Id);
            Assert.Equal("Belgrano, CABA", detalle.OrigenDir);
            Assert.Equal("Lomas de Zamora, Buenos Aires", detalle.DestinoDir);
            Assert.Equal(new[] { DiaSemana.Lunes, DiaSemana.Miercoles, DiaSemana.Viernes }, detalle.Dias);
            Assert.Equal(3, detalle.MaxPaquetes);
            Assert.Equal(10m, detalle.PesoMaxKg);
            Assert.Equal(5m, detalle.DesvioMaxKm);
            Assert.True(detalle.Activo);
            Assert.Equal(new[] { "Caballito, CABA", "Barracas, CABA" }, detalle.Paradas.Select(p => p.Direccion));
            Assert.Single(detalle.PaquetesAsignados);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_ViajePropio_DevuelveCoordenadasYSinEnviosPendientes()
        {
            var repositorio = new ViajeRepositorioFalso();
            repositorio.Viajes.Add(CrearViaje(id: 5, usuarioId: UsuarioId));
            var servicio = new ViajeServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.NotNull(detalle);
            Assert.Equal(-34.5627m, detalle.OrigenLat);
            Assert.Equal(-58.4063m, detalle.DestinoLng);
            Assert.False(detalle.TieneEnviosPendientes);
        }

        [Theory]
        [InlineData(EstadoEnvio.ConductorAsignado)]
        [InlineData(EstadoEnvio.Retirado)]
        [InlineData(EstadoEnvio.EnCamino)]
        public async Task ObtenerDetalleAsync_ConEnvioSinTerminar_InformaEnviosPendientes(EstadoEnvio estado)
        {
            var repositorio = new ViajeRepositorioFalso();
            repositorio.Viajes.Add(CrearViaje(id: 5, usuarioId: UsuarioId, estadosAsignados: estado));
            var servicio = new ViajeServicio(repositorio);

            var detalle = await servicio.ObtenerDetalleAsync(UsuarioId, 5);

            Assert.NotNull(detalle);
            Assert.True(detalle.TieneEnviosPendientes);
        }

        [Fact]
        public async Task ActualizarAsync_ViajeInexistente_DevuelveNoEncontrado()
        {
            var servicio = new ViajeServicio(new ViajeRepositorioFalso());

            var resultado = await servicio.ActualizarAsync(UsuarioId, 99, CrearDtoEdicion());

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado);
        }

        [Fact]
        public async Task ActualizarAsync_ViajeDeOtroUsuario_DevuelveNoEncontradoYNoLoModifica()
        {
            var repositorio = new ViajeRepositorioFalso();
            var viaje = CrearViaje(id: 5, usuarioId: 2);
            repositorio.Viajes.Add(viaje);
            var servicio = new ViajeServicio(repositorio);

            var resultado = await servicio.ActualizarAsync(UsuarioId, 5, CrearDtoEdicion());

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado);
            Assert.Equal("Belgrano, CABA", viaje.OrigenDir);
            Assert.Equal(0, repositorio.CantidadGuardados);
        }

        [Fact]
        public async Task ActualizarAsync_ConEnviosPendientes_DevuelveConflictoYNoLoModifica()
        {
            var repositorio = new ViajeRepositorioFalso();
            var viaje = CrearViaje(id: 5, usuarioId: UsuarioId, estadosAsignados: new[] { EstadoEnvio.Entregado, EstadoEnvio.Retirado });
            repositorio.Viajes.Add(viaje);
            var servicio = new ViajeServicio(repositorio);

            var resultado = await servicio.ActualizarAsync(UsuarioId, 5, CrearDtoEdicion());

            Assert.Equal(ResultadoOperacion.Conflicto, resultado);
            Assert.Equal("Belgrano, CABA", viaje.OrigenDir);
            Assert.Equal(3, viaje.MaxPaquetes);
            Assert.Equal(2, viaje.Paradas.Count);
            Assert.Equal(0, repositorio.CantidadGuardados);
        }

        [Fact]
        public async Task ActualizarAsync_ConEnviosFinalizados_PermiteEditar()
        {
            var repositorio = new ViajeRepositorioFalso();
            repositorio.Viajes.Add(CrearViaje(id: 5, usuarioId: UsuarioId,
                estadosAsignados: new[] { EstadoEnvio.Entregado, EstadoEnvio.Cancelado }));
            var servicio = new ViajeServicio(repositorio);

            var resultado = await servicio.ActualizarAsync(UsuarioId, 5, CrearDtoEdicion());

            Assert.Equal(ResultadoOperacion.Exito, resultado);
        }

        [Fact]
        public async Task ActualizarAsync_ViajePropio_ActualizaDatosDiasYParadasEnOrden()
        {
            var repositorio = new ViajeRepositorioFalso();
            var viaje = CrearViaje(id: 5, usuarioId: UsuarioId);
            var lunes = viaje.Dias.Single(d => d.DiaSemana == DiaSemana.Lunes);
            repositorio.Viajes.Add(viaje);
            var servicio = new ViajeServicio(repositorio);

            var resultado = await servicio.ActualizarAsync(UsuarioId, 5, CrearDtoEdicion());

            Assert.Equal(ResultadoOperacion.Exito, resultado);
            Assert.Equal(1, repositorio.CantidadGuardados);
            Assert.Equal("Palermo, CABA", viaje.OrigenDir);
            Assert.Equal(-34.5889m, viaje.OrigenLat);
            Assert.Equal("San Isidro, Buenos Aires", viaje.DestinoDir);
            Assert.Equal(new TimeOnly(18, 30), viaje.HoraSalida);
            Assert.Equal(new TimeOnly(19, 20), viaje.HoraLlegada);
            Assert.Equal(2, viaje.MaxPaquetes);
            Assert.Equal(6m, viaje.PesoMaxKg);
            Assert.Equal(2.5m, viaje.DesvioMaxKm);
            Assert.True(viaje.Activo);
            Assert.Equal(new[] { DiaSemana.Lunes, DiaSemana.Viernes }, viaje.Dias.Select(d => d.DiaSemana).OrderBy(d => d));
            // El día que se mantiene es la misma entidad (no se borra y reinserta).
            Assert.Contains(lunes, viaje.Dias);
            Assert.Equal(new[] { "Recoleta, CABA", "Belgrano, CABA" }, viaje.Paradas.Select(p => p.Direccion));
            Assert.Equal(new[] { 1, 2 }, viaje.Paradas.Select(p => p.Orden));
        }

        private static CrearViajeDto CrearDtoEdicion()
        {
            return new CrearViajeDto
            {
                OrigenDir = " Palermo, CABA ",
                OrigenLat = -34.5889m,
                OrigenLng = -58.4306m,
                DestinoDir = "San Isidro, Buenos Aires",
                DestinoLat = -34.4708m,
                DestinoLng = -58.5286m,
                Paradas = new List<DireccionDto>
                {
                    new DireccionDto { Direccion = "Recoleta, CABA", Lat = -34.5875m, Lng = -58.3974m },
                    new DireccionDto { Direccion = "Belgrano, CABA", Lat = -34.5627m, Lng = -58.4565m }
                },
                Dias = new List<DiaSemana> { DiaSemana.Viernes, DiaSemana.Lunes, DiaSemana.Viernes },
                HoraSalida = new TimeOnly(18, 30),
                HoraLlegada = new TimeOnly(19, 20),
                MaxPaquetes = 2,
                PesoMaxKg = 6m,
                DesvioMaxKm = 2.5m
            };
        }

        private static Viaje CrearViaje(int id, int usuarioId, params EstadoEnvio[] estadosAsignados)
        {
            return new Viaje
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = "Belgrano, CABA",
                OrigenLat = -34.5627m,
                OrigenLng = -58.4565m,
                DestinoDir = "Lomas de Zamora, Buenos Aires",
                DestinoLat = -34.7609m,
                DestinoLng = -58.4063m,
                HoraSalida = new TimeOnly(8, 0),
                HoraLlegada = new TimeOnly(8, 55),
                MaxPaquetes = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 5m,
                Activo = true,
                // Desordenados a propósito: el servicio debe ordenarlos.
                Dias = new List<ViajeDia>
                {
                    new ViajeDia { DiaSemana = DiaSemana.Viernes },
                    new ViajeDia { DiaSemana = DiaSemana.Lunes },
                    new ViajeDia { DiaSemana = DiaSemana.Miercoles }
                },
                Paradas = new List<Parada>
                {
                    new Parada { Orden = 2, Direccion = "Barracas, CABA" },
                    new Parada { Orden = 1, Direccion = "Caballito, CABA" }
                },
                Asignaciones = estadosAsignados
                    .Select(estado => new Asignacion { Envio = new Envio { Estado = estado } })
                    .ToList()
            };
        }

        private class ViajeRepositorioFalso : IViajeRepositorio
        {
            public List<Viaje> Viajes { get; } = new List<Viaje>();
            public int CantidadGuardados { get; private set; }

            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null)
            {
                var viajes = Viajes
                    .Where(v => v.UsuarioId == usuarioId && (!activo.HasValue || v.Activo == activo.Value))
                    .ToList();
                return Task.FromResult(viajes);
            }

            public Task<Viaje?> ObtenerPorIdAsync(int id)
            {
                return Task.FromResult(Viajes.FirstOrDefault(v => v.Id == id));
            }

            public Task AgregarAsync(Viaje viaje)
            {
                Viajes.Add(viaje);
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
