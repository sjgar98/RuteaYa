//using RuteaYa.API.Servicios;
//using RuteaYa.DTOs;
//using RuteaYa.DTOs.Enums;
//using RuteaYa.Entidades;
//using RuteaYa.Repositorios.Interfaces;

//namespace RuteaYa.Tests
//{
//    public class HistorialServicioTests
//    {
//        private const int UsuarioId = 1;

//        [Fact]
//        public async Task ObtenerHistorialAsync_SinAsignaciones_DevuelveMetricasEnCeroEItemsVacios()
//        {
//            var asignacionRepo = new AsignacionRepositorioFalso();
//            var viajeRepo = new ViajeRepositorioFalso();
//            var servicio = new HistorialServicio(asignacionRepo, viajeRepo);

//            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, null, null);

//            Assert.NotNull(resultado);
//            Assert.Equal(0, resultado.ViajesRealizados);
//            Assert.Equal(0, resultado.EnviosTransportados);
//            Assert.Equal(0m, resultado.GananciaEfectiva);
//            Assert.Empty(resultado.Items);
//        }

//        [Fact]
//        public async Task ObtenerHistorialAsync_ConAsignacionesEntregadas_CalculaMetricasYAgrupaCorrectamente()
//        {
//            var asignacionRepo = new AsignacionRepositorioFalso();
//            var viajeRepo = new ViajeRepositorioFalso();

//            var viaje = new Viaje
//            {
//                Id = 10,
//                UsuarioId = UsuarioId,
//                OrigenDir = "Palermo, CABA",
//                DestinoDir = "Obelisco, CABA",
//                Activo = true
//            };
//            viajeRepo.Viajes.Add(viaje);

//            var envio = new Envio
//            {
//                Id = 100,
//                UsuarioId = 2,
//                Estado = EstadoEnvio.Entregado,
//                Monto = 5000m
//            };

//            var asignacion = new Asignacion
//            {
//                Id = 1,
//                ViajeId = 10,
//                EnvioId = 100,
//                AsignadoEn = new DateTime(2026, 11, 2, 10, 0, 0, DateTimeKind.Utc),
//                Ganancia = 5000m,
//                Viaje = viaje,
//                Envio = envio
//            };
//            asignacionRepo.Asignaciones.Add(asignacion);

//            var servicio = new HistorialServicio(asignacionRepo, viajeRepo);
//            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, null, null);

//            Assert.NotNull(resultado);
//            Assert.Equal(1, resultado.ViajesRealizados);
//            Assert.Equal(1, resultado.EnviosTransportados);
//            Assert.Equal(5000m, resultado.GananciaEfectiva);

//            var item = Assert.Single(resultado.Items);
//            Assert.Equal(10, item.Id);
//            Assert.Equal("Viaje", item.Tipo);
//            Assert.Equal("Palermo, CABA → Obelisco, CABA", item.Descripcion);
//            Assert.Equal(5000m, item.Monto);
//        }

//        private class AsignacionRepositorioFalso : IAsignacionRepositorio
//        {
//            public List<Asignacion> Asignaciones { get; } = new List<Asignacion>();

//            public Task<List<Asignacion>> ListarEntregadasPorUsuarioAsync(int usuarioId, DateTime? desde, DateTime? hasta)
//            {
//                var query = Asignaciones
//                    .Where(a => a.Viaje.UsuarioId == usuarioId && a.Envio.Estado == EstadoEnvio.Entregado);

//                if (desde.HasValue)
//                    query = query.Where(a => a.AsignadoEn >= desde.Value);
//                if (hasta.HasValue)
//                    query = query.Where(a => a.AsignadoEn <= hasta.Value);

//                return Task.FromResult(query.ToList());
//            }

//            public Task<Asignacion?> ObtenerPorIdAsync(int id) => Task.FromResult(Asignaciones.FirstOrDefault(a => a.Id == id));
//            public Task<List<Asignacion>> ListarPorConductorAsync(int conductorId) => Task.FromResult(Asignaciones.Where(a => a.Viaje.UsuarioId == conductorId).ToList());
//            public Task GuardarCambiosAsync() => Task.CompletedTask;
//            public Task AgregarAsync(Asignacion asignacion) { Asignaciones.Add(asignacion); return Task.CompletedTask; }
//            public Task AgregarAsync(List<Asignacion> asignaciones) { Asignaciones.AddRange(asignaciones); return Task.CompletedTask; }
//        }

//        private class ViajeRepositorioFalso : IViajeRepositorio
//        {
//            public List<Viaje> Viajes { get; } = new List<Viaje>();

//            public Task<List<Viaje>> ListarActivosPorUsuarioAsync(int usuarioId)
//            {
//                return Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId && v.Activo).ToList());
//            }

//            public Task<Viaje?> ObtenerPorIdAsync(int id) => Task.FromResult(Viajes.FirstOrDefault(v => v.Id == id));
//            public Task AgregarAsync(Viaje viaje) { Viajes.Add(viaje); return Task.CompletedTask; }
//            public Task GuardarCambiosAsync() => Task.CompletedTask;
//            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId) => Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId).ToList());
//            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null) => Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId).ToList());
//        }
//    }
//}