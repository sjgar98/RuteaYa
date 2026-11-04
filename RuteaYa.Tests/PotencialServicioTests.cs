using RuteaYa.API.Servicios;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class PotencialServicioTests
    {
        private const int UsuarioId = 1;

        [Fact]
        public async Task ObtenerPotencialAsync_SinViajesActivos_DevuelveEstructuraVacia()
        {
            var viajeRepo = new ViajeRepositorioFalso();
            var envioRepo = new EnvioRepositorioFalso();
            var servicio = new PotencialServicio(viajeRepo, envioRepo);

            var resultado = await servicio.ObtenerPotencialAsync(UsuarioId, "2026-11", "ganancia");

            Assert.NotNull(resultado);
            Assert.Equal("2026-11", resultado.Mes);
            Assert.Equal("ganancia", resultado.Criterio);
            Assert.Equal(0m, resultado.GananciaPotencial);
            Assert.Empty(resultado.MejorRecorrido);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_CriterioTodos_DevuelveTodasLasOportunidadesCompatibles()
        {
            var viajeRepo = new ViajeRepositorioFalso();
            viajeRepo.Viajes.Add(CrearViaje(id: 1, usuarioId: UsuarioId, distancia: 10m, desvioMax: 5m));

            var envioRepo = new EnvioRepositorioFalso();
            envioRepo.Envios.Add(CrearEnvio(id: 101, usuarioId: 2, monto: 5000m, fecha: new DateOnly(2026, 11, 10)));
            envioRepo.Envios.Add(CrearEnvio(id: 102, usuarioId: 2, monto: 8000m, fecha: new DateOnly(2026, 11, 15)));

            var servicio = new PotencialServicio(viajeRepo, envioRepo);

            var resultado = await servicio.ObtenerPotencialAsync(UsuarioId, "2026-11", "todos");

            Assert.NotNull(resultado);
            Assert.Equal("todos", resultado.Criterio);
            Assert.Equal(2, resultado.PaquetesPosibles);
            Assert.Equal(13000m, resultado.GananciaPotencial);
            Assert.Equal(2, resultado.MejorRecorrido.Count);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_CriterioGanancia_PriorizaMayorMonto()
        {
            var viajeRepo = new ViajeRepositorioFalso();
            viajeRepo.Viajes.Add(CrearViaje(id: 1, usuarioId: UsuarioId, distancia: 10m, desvioMax: 5m));

            var envioRepo = new EnvioRepositorioFalso();
            // Envío con menor ganancia
            envioRepo.Envios.Add(CrearEnvio(id: 101, usuarioId: 2, monto: 4000m, fecha: new DateOnly(2026, 11, 10)));
            // Envío con mayor ganancia para el mismo viaje disponible
            envioRepo.Envios.Add(CrearEnvio(id: 102, usuarioId: 2, monto: 12000m, fecha: new DateOnly(2026, 11, 12)));

            var servicio = new PotencialServicio(viajeRepo, envioRepo);

            var resultado = await servicio.ObtenerPotencialAsync(UsuarioId, "2026-11", "ganancia");

            Assert.NotNull(resultado);
            Assert.Equal("ganancia", resultado.Criterio);
            Assert.Equal(1, resultado.PaquetesPosibles);
            Assert.Equal(12000m, resultado.GananciaPotencial);
            var itemSeleccionado = Assert.Single(resultado.MejorRecorrido);
            Assert.Equal(12000m, itemSeleccionado.GananciaEstimada);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_CriterioDesvio_FiltraDesviosAltosYPriorizaMinimo()
        {
            var viajeRepo = new ViajeRepositorioFalso();
            viajeRepo.Viajes.Add(CrearViaje(id: 1, usuarioId: UsuarioId, distancia: 10m, desvioMax: 10m, lat1: -34.60m, lon1: -58.38m, lat2: -34.70m, lon2: -58.48m));

            var envioRepo = new EnvioRepositorioFalso();
            // Envío lejano (generará mayor desvío)
            envioRepo.Envios.Add(CrearEnvio(id: 101, usuarioId: 2, monto: 15000m, lat1: -34.50m, lon1: -58.20m, lat2: -34.90m, lon2: -58.90m, fecha: new DateOnly(2026, 11, 5)));
            // Envío muy cercano a la ruta (generará menor desvío, menor o igual a 2km)
            envioRepo.Envios.Add(CrearEnvio(id: 102, usuarioId: 2, monto: 5000m, lat1: -34.60m, lon1: -58.38m, lat2: -34.61m, lon2: -58.39m, fecha: new DateOnly(2026, 11, 6)));

            var servicio = new PotencialServicio(viajeRepo, envioRepo);

            var resultado = await servicio.ObtenerPotencialAsync(UsuarioId, "2026-11", "desvio");

            Assert.NotNull(resultado);
            Assert.Equal("desvio", resultado.Criterio);
            // Debe filtrar y retener el de menor desvío descartando el excesivo
            var item = Assert.Single(resultado.MejorRecorrido);
            Assert.Equal(5000m, item.GananciaEstimada);
            Assert.True(item.DesvioTotalKm <= 2.0m);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_NoCuentaEnviosIncompatiblesPorPeso_ODevuelveVacio()
        {
            var viajeRepo = new ViajeRepositorioFalso();
            viajeRepo.Viajes.Add(CrearViaje(id: 1, usuarioId: UsuarioId, pesoMax: 2m));

            var envioRepo = new EnvioRepositorioFalso();
            // Envío con peso superior al permitido por el viaje
            envioRepo.Envios.Add(CrearEnvio(id: 101, usuarioId: 2, peso: 5m, monto: 10000m, fecha: new DateOnly(2026, 11, 10)));

            var servicio = new PotencialServicio(viajeRepo, envioRepo);

            var resultado = await servicio.ObtenerPotencialAsync(UsuarioId, "2026-11", "ganancia");

            Assert.NotNull(resultado);
            Assert.Equal(0m, resultado.GananciaPotencial);
            Assert.Empty(resultado.MejorRecorrido);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_NoDuplicaEnvioEnDistintosViajes()
        {
            var viajeRepo = new ViajeRepositorioFalso();
            // Dos viajes activos
            viajeRepo.Viajes.Add(CrearViaje(id: 1, usuarioId: UsuarioId, distancia: 10m, desvioMax: 20m));
            viajeRepo.Viajes.Add(CrearViaje(id: 2, usuarioId: UsuarioId, distancia: 10m, desvioMax: 20m));

            var envioRepo = new EnvioRepositorioFalso();
            // Un único envío disponible compatible con ambos
            envioRepo.Envios.Add(CrearEnvio(id: 101, usuarioId: 2, monto: 6000m, fecha: new DateOnly(2026, 11, 10)));

            var servicio = new PotencialServicio(viajeRepo, envioRepo);

            var resultado = await servicio.ObtenerPotencialAsync(UsuarioId, "2026-11", "todos");

            Assert.NotNull(resultado);
            // El envío 101 debe asignarse a un solo viaje, evitando duplicarse en ambos
            var asignacionesAlEnvio = resultado.MejorRecorrido.Count;
            Assert.Equal(1, asignacionesAlEnvio);
            Assert.Equal(6000m, resultado.GananciaPotencial);
        }

        private static Viaje CrearViaje(int id, int usuarioId, decimal distancia = 10m, decimal desvioMax = 5m, decimal pesoMax = 10m, decimal lat1 = -34.5810m, decimal lon1 = -58.4208m, decimal lat2 = -34.6037m, decimal lon2 = -58.3816m)
        {
            return new Viaje
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = "Caballito, CABA",
                OrigenLat = lat1,
                OrigenLng = lon1,
                DestinoDir = "Centro, CABA",
                DestinoLat = lat2,
                DestinoLng = lon2,
                DistanciaKm = distancia,
                DesvioMaxKm = desvioMax,
                PesoMaxKg = pesoMax,
                Activo = true
            };
        }

        private static Envio CrearEnvio(int id, int usuarioId, decimal monto = 4000m, decimal peso = 1m, decimal lat1 = -34.5810m, decimal lon1 = -58.4208m, decimal lat2 = -34.6037m, decimal lon2 = -58.3816m, DateOnly? fecha = null)
        {
            return new Envio
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = "Origen Envío",
                OrigenLat = lat1,
                OrigenLng = lon1,
                DestinoDir = "Destino Envío",
                DestinoLat = lat2,
                DestinoLng = lon2,
                FechaLimite = fecha ?? new DateOnly(2026, 11, 20),
                PesoKg = peso,
                Tamano = TamanoEnvio.Pequeno,
                Descripcion = "Test Paquete",
                Monto = monto,
                Estado = EstadoEnvio.BuscandoConductor,
                CreadoEn = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }

        // Dobles de prueba falsos (Mocks simulados estilo repositorio)
        private class ViajeRepositorioFalso : IViajeRepositorio
        {
            public List<Viaje> Viajes { get; } = new List<Viaje>();

            public Task<List<Viaje>> ListarActivosPorUsuarioAsync(int usuarioId)
            {
                return Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId && v.Activo).ToList());
            }

            public Task<Viaje?> ObtenerPorIdAsync(int id) => Task.FromResult(Viajes.FirstOrDefault(v => v.Id == id));
            public Task AgregarAsync(Viaje viaje) { Viajes.Add(viaje); return Task.CompletedTask; }
            public Task GuardarCambiosAsync() => Task.CompletedTask;
            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId) => Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId).ToList());

            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null)
            {
                throw new NotImplementedException();
            }
        }

        private class EnvioRepositorioFalso : IEnvioRepositorio
        {
            public List<Envio> Envios { get; } = new List<Envio>();

            public Task<List<Envio>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null)
            {
                return Task.FromResult(Envios.Where(e => e.UsuarioId == usuarioId).ToList());
            }

            public Task<List<Envio>> ListarDisponiblesAsync(DateOnly desde, int usuarioIdExcluido)
            {
                return Task.FromResult(Envios.Where(e => e.Estado == EstadoEnvio.BuscandoConductor && e.UsuarioId != usuarioIdExcluido).ToList());
            }

            public Task<List<Envio>> ListarDisponiblesRangoAsync(DateOnly desde, DateOnly hasta, int usuarioIdExcluido)
            {
                return Task.FromResult(Envios.Where(e => e.Estado == EstadoEnvio.BuscandoConductor
                                                       && e.UsuarioId != usuarioIdExcluido
                                                       && e.FechaLimite >= desde
                                                       && e.FechaLimite <= hasta).ToList());
            }

            public Task<List<Envio>> ListarPorIdsAsync(List<int> ids) => Task.FromResult(Envios.Where(e => ids.Contains(e.Id)).ToList());
            public Task<Envio?> ObtenerPorIdAsync(int id) => Task.FromResult(Envios.FirstOrDefault(e => e.Id == id));
            public Task AgregarAsync(Envio envio) { Envios.Add(envio); return Task.CompletedTask; }
            public Task GuardarCambiosAsync() => Task.CompletedTask;
        }
    }
}