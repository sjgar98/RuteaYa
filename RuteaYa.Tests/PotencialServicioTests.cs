using Microsoft.Extensions.Logging.Abstractions;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Rutas;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class PotencialServicioTests
    {
        private const int ConductorId = 1;
        private const int RemitenteId = 2;

        private const decimal OrigenLat = -34.5627m, OrigenLng = -58.4583m;
        private const decimal DestinoLat = -34.7608m, DestinoLng = -58.4060m;

        private static readonly DateOnly Hoy = new DateOnly(2026, 11, 10);
        private const string MesActual = "2026-11";

        private readonly ViajeRepositorioFalso _viajes = new ViajeRepositorioFalso();
        private readonly EnvioRepositorioFalso _envios = new EnvioRepositorioFalso();
        private readonly RutaServicioFalso _rutas;

        public PotencialServicioTests()
        {
            _rutas = new RutaServicioFalso(_envios);
        }

        private PotencialServicio CrearServicio()
        {
            var oportunidades = new OportunidadesServicio(
                _viajes, _envios, new AsignacionRepositorioFalso(), _rutas,
                NullLogger<OportunidadesServicio>.Instance);
            var optimizador = new OptimizadorServicio(
                _viajes, _envios, oportunidades,
                NullLogger<OptimizadorServicio>.Instance);
            return new PotencialServicio(_viajes, optimizador);
        }

        [Theory]
        [InlineData("2026-13")]
        [InlineData("noviembre")]
        [InlineData("")]
        public async Task ObtenerPotencialAsync_MesInvalido_DevuelveNull(string mes)
        {
            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, mes, "ganancia", Hoy);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_MesPasado_DevuelveVacio()
        {
            _viajes.Viajes.Add(CrearViaje(10));
            AgregarEnvio(1, 4000m, 2m, 1m);

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, "2026-10", "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(0, resultado!.ViajesAnalizados);
            Assert.Equal(0m, resultado.GananciaPotencial);
            Assert.Empty(resultado.MejorRecorrido);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_EjemploDelEnunciado_UsaLaCombinacionOptima()
        {
            _viajes.Viajes.Add(CrearViaje(10));
            AgregarEjemploSeccion12();

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado!.ViajesAnalizados);
            Assert.Equal(3, resultado.PaquetesPosibles);
            Assert.Equal(12000m, resultado.GananciaPotencial);
            var fila = Assert.Single(resultado.MejorRecorrido);
            Assert.Equal(9m, fila.PesoTotalKg);
            Assert.Equal(3.5m, fila.DesvioTotalKm);
            Assert.Equal(new[] { 1, 2, 5 }, resultado.DetalleAsignaciones.Select(d => d.EnvioId).OrderBy(id => id));
        }

        [Fact]
        public async Task ObtenerPotencialAsync_CriterioDesvio_EligeElMenorDesvio()
        {
            _viajes.Viajes.Add(CrearViaje(10));
            AgregarEjemploSeccion12();

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "desvio", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal("desvio", resultado!.Criterio);
            var fila = Assert.Single(resultado.MejorRecorrido);
            Assert.Equal(0.5m, fila.DesvioTotalKm);
            Assert.Equal(2500m, resultado.GananciaPotencial);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_CalculaPromedioYGananciaPorSemana()
        {
            _viajes.Viajes.Add(CrearViaje(10));
            _viajes.Viajes.Add(CrearViaje(11));
            AgregarEnvio(1, 6000m, 2m, 1m);
            AgregarEnvio(2, 6000m, 2m, 1m);
            _viajes.Viajes[1].MaxPaquetes = 1;
            _viajes.Viajes[0].MaxPaquetes = 1;

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado!.ViajesAnalizados);
            Assert.Equal(12000m, resultado.GananciaPotencial);
            Assert.Equal(6000m, resultado.PromedioPorViaje);
            Assert.Equal(4000m, resultado.GananciaPorSemana);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_NoDuplicaEnvioEnDistintosViajes()
        {
            _viajes.Viajes.Add(CrearViaje(10));
            _viajes.Viajes.Add(CrearViaje(11));
            AgregarEnvio(1, 5000m, 2m, 1m);

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado!.ViajesAnalizados);
            Assert.Equal(1, resultado.PaquetesPosibles);
            Assert.Equal(5000m, resultado.GananciaPotencial);
            Assert.Equal(10, Assert.Single(resultado.MejorRecorrido).ViajeId);
            Assert.Single(resultado.DetalleAsignaciones);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_IgnoraViajesInactivos()
        {
            var viaje = CrearViaje(10);
            viaje.Activo = false;
            _viajes.Viajes.Add(viaje);
            AgregarEnvio(1, 5000m, 2m, 1m);

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(0, resultado!.ViajesAnalizados);
            Assert.Equal(0m, resultado.GananciaPotencial);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_ViajeSinSalidasEnElPeriodo_NoSeAnaliza()
        {
            _viajes.Viajes.Add(CrearViaje(10, DiaSemana.Martes));
            AgregarEnvio(1, 5000m, 2m, 1m, fechaLimite: new DateOnly(2026, 12, 31));

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "ganancia", new DateOnly(2026, 11, 25));

            Assert.NotNull(resultado);
            Assert.Equal(0, resultado!.ViajesAnalizados);
            Assert.Empty(resultado.MejorRecorrido);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_EnvioConFechaLimitePosteriorAlMes_Cuenta()
        {
            _viajes.Viajes.Add(CrearViaje(10));
            AgregarEnvio(1, 5000m, 2m, 1m, fechaLimite: new DateOnly(2027, 1, 15));

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, "2026-12", "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(5000m, resultado!.GananciaPotencial);
            Assert.Equal(new DateOnly(2026, 12, 1), Assert.Single(resultado.DetalleAsignaciones).FechaViaje);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_RespetaCapacidadYPeso()
        {
            _viajes.Viajes.Add(CrearViaje(10, maxPaquetes: 1, pesoMaxKg: 3m));
            AgregarEnvio(1, 9000m, 5m, 1m);
            AgregarEnvio(2, 3000m, 2m, 1m);
            AgregarEnvio(3, 2000m, 1m, 1m);

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado!.PaquetesPosibles);
            Assert.Equal(3000m, resultado.GananciaPotencial);
        }

        [Fact]
        public async Task ObtenerPotencialAsync_NoCuentaEnviosConDesvioExcesivo()
        {
            _viajes.Viajes.Add(CrearViaje(10));
            AgregarEnvio(1, 9000m, 2m, 8m);

            var resultado = await CrearServicio().ObtenerPotencialAsync(ConductorId, MesActual, "ganancia", Hoy);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado!.ViajesAnalizados);
            Assert.Equal(0m, resultado.GananciaPotencial);
            Assert.Empty(resultado.MejorRecorrido);
        }

        private void AgregarEjemploSeccion12()
        {
            AgregarEnvio(1, 4000m, 2m, 1.0m);
            AgregarEnvio(2, 5500m, 6m, 2.0m);
            AgregarEnvio(3, 3500m, 4m, 1.0m);
            AgregarEnvio(4, 7000m, 8m, 3.0m);
            AgregarEnvio(5, 2500m, 1m, 0.5m);
        }

        private void AgregarEnvio(int id, decimal monto, decimal pesoKg, decimal desvioKm, DateOnly? fechaLimite = null)
        {
            var fraccion = 0.2m + id / 10000m;
            _envios.Envios.Add(new Envio
            {
                Id = id,
                UsuarioId = RemitenteId,
                OrigenDir = $"Retiro {id}",
                OrigenLat = OrigenLat + (DestinoLat - OrigenLat) * fraccion,
                OrigenLng = OrigenLng + (DestinoLng - OrigenLng) * fraccion,
                DestinoDir = $"Entrega {id}",
                DestinoLat = OrigenLat + (DestinoLat - OrigenLat) * 0.75m,
                DestinoLng = OrigenLng + (DestinoLng - OrigenLng) * 0.75m,
                FechaLimite = fechaLimite ?? Hoy.AddDays(14),
                PesoKg = pesoKg,
                Tamano = TamanoEnvio.Pequeno,
                Descripcion = $"Paquete {id}",
                Monto = monto,
                Estado = EstadoEnvio.BuscandoConductor
            });
            _rutas.Desvios[id] = desvioKm;
        }

        private static Viaje CrearViaje(int id, DiaSemana? unicoDia = null, int maxPaquetes = 3, decimal pesoMaxKg = 10m)
        {
            var viaje = new Viaje
            {
                Id = id,
                UsuarioId = ConductorId,
                OrigenDir = "Belgrano, CABA",
                OrigenLat = OrigenLat,
                OrigenLng = OrigenLng,
                DestinoDir = "Lomas de Zamora, Buenos Aires",
                DestinoLat = DestinoLat,
                DestinoLng = DestinoLng,
                HoraSalida = new TimeOnly(8, 0),
                HoraLlegada = new TimeOnly(8, 55),
                MaxPaquetes = maxPaquetes,
                PesoMaxKg = pesoMaxKg,
                DesvioMaxKm = 5m,
                Activo = true
            };

            var dias = unicoDia.HasValue ? new[] { unicoDia.Value } : Enum.GetValues<DiaSemana>();
            foreach (var dia in dias)
            {
                viaje.Dias.Add(new ViajeDia { ViajeId = id, DiaSemana = dia });
            }

            return viaje;
        }

        private class ViajeRepositorioFalso : IViajeRepositorio
        {
            public List<Viaje> Viajes { get; } = new List<Viaje>();

            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null)
                => Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId && (activo == null || v.Activo == activo)).ToList());

            public Task<Viaje?> ObtenerPorIdAsync(int id) => Task.FromResult(Viajes.FirstOrDefault(v => v.Id == id));
            public Task AgregarAsync(Viaje viaje) { Viajes.Add(viaje); return Task.CompletedTask; }
            public Task GuardarCambiosAsync() => Task.CompletedTask;
        }

        private class EnvioRepositorioFalso : IEnvioRepositorio
        {
            public List<Envio> Envios { get; } = new List<Envio>();

            public Task<List<Envio>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null)
                => Task.FromResult(Envios.Where(e => e.UsuarioId == usuarioId && (estado == null || e.Estado == estado)).ToList());

            public Task<List<Envio>> ListarDisponiblesAsync(DateOnly desde, int usuarioIdExcluido)
                => Task.FromResult(Envios
                    .Where(e => e.Estado == EstadoEnvio.BuscandoConductor && e.FechaLimite >= desde && e.UsuarioId != usuarioIdExcluido)
                    .OrderBy(e => e.FechaLimite)
                    .ToList());

            public Task<List<Envio>> ListarPorIdsAsync(List<int> ids) => Task.FromResult(Envios.Where(e => ids.Contains(e.Id)).ToList());
            public Task<List<Envio>> ListarFinalizadosPorUsuarioAsync(int usuarioId) => throw new NotImplementedException();
            public Task<Envio?> ObtenerPorIdAsync(int id) => Task.FromResult(Envios.FirstOrDefault(e => e.Id == id));
            public Task AgregarAsync(Envio envio) { Envios.Add(envio); return Task.CompletedTask; }
            public Task GuardarCambiosAsync() => Task.CompletedTask;
        }

        private class AsignacionRepositorioFalso : IAsignacionRepositorio
        {
            public Task<List<Asignacion>> ListarPorConductorAsync(int conductorId) => Task.FromResult(new List<Asignacion>());
            public Task<List<Asignacion>> ListarEntregadasPorConductorAsync(int conductorId, DateOnly? desde, DateOnly? hasta) => throw new NotImplementedException();
            public Task<Asignacion?> ObtenerPorIdAsync(int id) => Task.FromResult<Asignacion?>(null);
            public Task AgregarAsync(List<Asignacion> asignaciones) => Task.CompletedTask;
            public Task GuardarCambiosAsync() => Task.CompletedTask;
        }

        private class RutaServicioFalso : IRutaServicio
        {
            private readonly EnvioRepositorioFalso _envios;

            public RutaServicioFalso(EnvioRepositorioFalso envios)
            {
                _envios = envios;
            }

            public Dictionary<int, decimal> Desvios { get; } = new Dictionary<int, decimal>();

            public Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(
                IReadOnlyList<CoordenadaDto> recorridoHabitual,
                CoordenadaDto retiro, CoordenadaDto entrega,
                CancellationToken cancelacion = default)
            {
                var envio = _envios.Envios.Single(e => e.OrigenLat == retiro.Latitud && e.OrigenLng == retiro.Longitud);
                var desvio = Desvios.GetValueOrDefault(envio.Id, 1m);
                return Task.FromResult(new DetalleOportunidadRutaDto
                {
                    DistanciaHabitualKm = 18.4m,
                    DistanciaConEnvioKm = 18.4m + desvio,
                    DesvioKm = desvio,
                    TiempoHabitualMinutos = 55,
                    TiempoAdicionalMinutos = (int)Math.Round(desvio * 3)
                });
            }

            public Task<RutaResultadoDto> CalcularRutaAsync(IReadOnlyList<CoordenadaDto> recorrido, CancellationToken cancelacion = default) => throw new NotSupportedException();
        }
    }
}
