using RuteaYa.API.Servicios;
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

        private static Viaje CrearViaje(int id, int usuarioId)
        {
            return new Viaje
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = "Belgrano, CABA",
                DestinoDir = "Lomas de Zamora, Buenos Aires",
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
                }
            };
        }

        private class ViajeRepositorioFalso : IViajeRepositorio
        {
            public List<Viaje> Viajes { get; } = new List<Viaje>();

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
                return Task.CompletedTask;
            }
        }
    }
}
