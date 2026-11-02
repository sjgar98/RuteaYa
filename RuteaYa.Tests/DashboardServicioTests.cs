using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;

namespace RuteaYa.Tests
{
    public class DashboardServicioTests
    {
        private const int UsuarioId = 1;

        // Lunes 2 de noviembre de 2026, 10:00.
        private static readonly DateTime _lunes10 = new DateTime(2026, 11, 2, 10, 0, 0);

        private readonly ViajeServicioFalso _viajes = new ViajeServicioFalso();
        private readonly EnvioServicioFalso _envios = new EnvioServicioFalso();
        private readonly OportunidadesServicioFalso _oportunidades = new OportunidadesServicioFalso();
        private readonly OptimizadorServicioFalso _optimizador = new OptimizadorServicioFalso();

        [Fact]
        public void CalcularProximasSalidas_HoyYaSalio_EmpiezaDesdeLaProximaFecha()
        {
            var viajes = new List<ViajeDto>
            {
                CrearViaje(1, new TimeOnly(8, 0), DiaSemana.Lunes, DiaSemana.Miercoles)
            };

            var salidas = DashboardServicio.CalcularProximasSalidas(viajes, _lunes10, 7);

            Assert.Equal(new[] { new DateOnly(2026, 11, 4) }, salidas.Select(s => s.Fecha));
        }

        [Fact]
        public void CalcularProximasSalidas_HoyTodaviaNoSalio_IncluyeHoy()
        {
            var viajes = new List<ViajeDto>
            {
                CrearViaje(1, new TimeOnly(18, 30), DiaSemana.Lunes)
            };

            var salidas = DashboardServicio.CalcularProximasSalidas(viajes, _lunes10, 7);

            Assert.Equal(new[] { new DateOnly(2026, 11, 2) }, salidas.Select(s => s.Fecha));
        }

        [Fact]
        public void CalcularProximasSalidas_VariosViajes_OrdenaPorFechaYHora()
        {
            var viajes = new List<ViajeDto>
            {
                CrearViaje(1, new TimeOnly(18, 0), DiaSemana.Martes),
                CrearViaje(2, new TimeOnly(7, 0), DiaSemana.Martes, DiaSemana.Viernes),
                CrearViaje(3, new TimeOnly(12, 0), DiaSemana.Lunes)
            };

            var salidas = DashboardServicio.CalcularProximasSalidas(viajes, _lunes10, 7);

            Assert.Equal(new[] { 3, 2, 1, 2 }, salidas.Select(s => s.ViajeId));
            Assert.Equal(new DateOnly(2026, 11, 6), salidas.Last().Fecha);
        }

        [Fact]
        public async Task ObtenerAsync_EnviosPendientes_ExcluyeEntregadosYCanceladosYOrdenaPorFecha()
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            _envios.Envios.AddRange(new[]
            {
                CrearEnvio(1, EstadoEnvio.EnCamino, hoy.AddDays(5)),
                CrearEnvio(2, EstadoEnvio.Entregado, hoy),
                CrearEnvio(3, EstadoEnvio.BuscandoConductor, hoy.AddDays(1)),
                CrearEnvio(4, EstadoEnvio.Cancelado, hoy),
                CrearEnvio(5, EstadoEnvio.ConductorAsignado, hoy.AddDays(3))
            });

            var dashboard = await CrearServicio().ObtenerAsync(UsuarioId);

            Assert.Equal(3, dashboard.CantidadEnviosPendientes);
            Assert.Equal(new[] { 3, 5, 1 }, dashboard.EnviosPendientes.Select(e => e.Id));
        }

        [Fact]
        public async Task ObtenerAsync_MasDeCincoEnviosPendientes_ListaCincoYCuentaTodos()
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            for (var i = 1; i <= 7; i++)
            {
                _envios.Envios.Add(CrearEnvio(i, EstadoEnvio.BuscandoConductor, hoy.AddDays(i)));
            }

            var dashboard = await CrearServicio().ObtenerAsync(UsuarioId);

            Assert.Equal(7, dashboard.CantidadEnviosPendientes);
            Assert.Equal(5, dashboard.EnviosPendientes.Count);
        }

        [Fact]
        public async Task ObtenerAsync_EnvioCompatibleConDosViajes_CuentaUnaSolaOportunidad()
        {
            _viajes.Viajes.Add(CrearViaje(1, new TimeOnly(8, 0), DiaSemana.Lunes));
            _viajes.Viajes.Add(CrearViaje(2, new TimeOnly(9, 0), DiaSemana.Martes));
            _oportunidades.PorViaje[1] = new List<int> { 10, 11 };
            _oportunidades.PorViaje[2] = new List<int> { 11, 12 };

            var dashboard = await CrearServicio().ObtenerAsync(UsuarioId);

            Assert.Equal(3, dashboard.CantidadOportunidades);
            Assert.Equal(2, dashboard.ViajesActivos);
        }

        [Fact]
        public async Task ObtenerAsync_SoloConsideraViajesActivos()
        {
            var inactivo = CrearViaje(1, new TimeOnly(8, 0), DiaSemana.Lunes);
            inactivo.Activo = false;
            _viajes.Viajes.Add(inactivo);
            _oportunidades.PorViaje[1] = new List<int> { 10 };
            _optimizador.PorViaje[1] = new List<OportunidadDto> { CrearOportunidad(10, 5000m, Hoy()) };

            var dashboard = await CrearServicio().ObtenerAsync(UsuarioId);

            Assert.Equal(0, dashboard.ViajesActivos);
            Assert.Equal(0, dashboard.CantidadOportunidades);
            Assert.Equal(0m, dashboard.PotencialMes);
        }

        [Fact]
        public async Task ObtenerAsync_Potencial_SumaLaOptimaDeCadaViajeSinRepetirEnvios()
        {
            _viajes.Viajes.Add(CrearViaje(1, new TimeOnly(8, 0), DiaSemana.Lunes));
            _viajes.Viajes.Add(CrearViaje(2, new TimeOnly(9, 0), DiaSemana.Martes));
            _optimizador.PorViaje[1] = new List<OportunidadDto>
            {
                CrearOportunidad(10, 4000m, Hoy()),
                CrearOportunidad(11, 5500m, Hoy())
            };
            // El envío 11 ya se contó en el viaje 1.
            _optimizador.PorViaje[2] = new List<OportunidadDto>
            {
                CrearOportunidad(11, 5500m, Hoy()),
                CrearOportunidad(12, 2500m, Hoy())
            };

            var dashboard = await CrearServicio().ObtenerAsync(UsuarioId);

            Assert.Equal(12000m, dashboard.PotencialMes);
            Assert.Equal(3, dashboard.PaquetesPotencialMes);
        }

        [Fact]
        public async Task ObtenerAsync_Potencial_NoCuentaEnviosDeOtroMes()
        {
            _viajes.Viajes.Add(CrearViaje(1, new TimeOnly(8, 0), DiaSemana.Lunes));
            _optimizador.PorViaje[1] = new List<OportunidadDto>
            {
                CrearOportunidad(10, 4000m, Hoy()),
                CrearOportunidad(11, 7000m, Hoy().AddMonths(1))
            };

            var dashboard = await CrearServicio().ObtenerAsync(UsuarioId);

            Assert.Equal(4000m, dashboard.PotencialMes);
            Assert.Equal(1, dashboard.PaquetesPotencialMes);
        }

        [Fact]
        public async Task ObtenerAsync_SinDatos_DevuelveTodoEnCero()
        {
            var dashboard = await CrearServicio().ObtenerAsync(UsuarioId);

            Assert.Equal(0, dashboard.CantidadProximasSalidas);
            Assert.Empty(dashboard.ProximasSalidas);
            Assert.Equal(0, dashboard.CantidadEnviosPendientes);
            Assert.Equal(0, dashboard.CantidadOportunidades);
            Assert.Equal(0m, dashboard.PotencialMes);
        }

        private DashboardServicio CrearServicio()
        {
            return new DashboardServicio(_viajes, _envios, _oportunidades, _optimizador);
        }

        // Misma "hoy" que usa el servicio (Argentina).
        private static DateOnly Hoy()
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));
        }

        private static ViajeDto CrearViaje(int id, TimeOnly horaSalida, params DiaSemana[] dias)
        {
            return new ViajeDto
            {
                Id = id,
                OrigenDir = "Belgrano, CABA",
                DestinoDir = "Lomas de Zamora, Buenos Aires",
                Dias = dias.ToList(),
                HoraSalida = horaSalida,
                HoraLlegada = horaSalida.AddMinutes(55),
                Activo = true
            };
        }

        private static EnvioDto CrearEnvio(int id, EstadoEnvio estado, DateOnly fechaLimite)
        {
            return new EnvioDto { Id = id, Estado = estado, FechaLimite = fechaLimite, Descripcion = "Paquete" };
        }

        private static OportunidadDto CrearOportunidad(int envioId, decimal monto, DateOnly fechaViaje)
        {
            return new OportunidadDto { EnvioId = envioId, MontoPago = monto, FechaViaje = fechaViaje };
        }

        private class ViajeServicioFalso : IViajeServicio
        {
            public List<ViajeDto> Viajes { get; } = new List<ViajeDto>();

            public Task<List<ViajeDto>> ListarPorUsuarioAsync(int usuarioId, bool? activo)
            {
                return Task.FromResult(Viajes.Where(v => !activo.HasValue || v.Activo == activo.Value).ToList());
            }

            public Task<ViajeDetalleDto?> ObtenerDetalleAsync(int usuarioId, int id) => throw new NotImplementedException();
            public Task<ViajeDto> CrearAsync(int usuarioId, CrearViajeDto dto) => throw new NotImplementedException();
            public Task<ResultadoOperacion> ActualizarAsync(int usuarioId, int id, CrearViajeDto dto) => throw new NotImplementedException();
            public Task<ResultadoOperacion> CambiarActivoAsync(int usuarioId, int id, bool activo) => throw new NotImplementedException();
            public Task<List<DireccionDto>> BuscarDireccionesAsync(string texto) => throw new NotImplementedException();
        }

        private class EnvioServicioFalso : IEnvioServicio
        {
            public List<EnvioDto> Envios { get; } = new List<EnvioDto>();

            public Task<List<EnvioDto>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado)
            {
                return Task.FromResult(Envios.Where(e => !estado.HasValue || e.Estado == estado.Value).ToList());
            }

            public Task<EnvioDetalleDto?> ObtenerDetalleAsync(int usuarioId, int id) => throw new NotImplementedException();
            public Task<EnvioDto> CrearAsync(int usuarioId, CrearEnvioDto dto) => throw new NotImplementedException();
            public Task<ResultadoOperacion> CancelarAsync(int usuarioId, int id) => throw new NotImplementedException();
        }

        private class OportunidadesServicioFalso : IOportunidadesServicio
        {
            // Ids de los envíos compatibles con cada viaje.
            public Dictionary<int, List<int>> PorViaje { get; } = new Dictionary<int, List<int>>();

            public Task<OportunidadesViajeDto?> ListarAsync(int usuarioId, int viajeId, string? ordenarPor = "ganancia")
            {
                var ids = PorViaje.GetValueOrDefault(viajeId, new List<int>());
                return Task.FromResult<OportunidadesViajeDto?>(new OportunidadesViajeDto
                {
                    ViajeId = viajeId,
                    Oportunidades = ids.Select(id => new OportunidadDto { EnvioId = id }).ToList()
                });
            }

            public Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int usuarioId, int viajeId, int envioId) => throw new NotImplementedException();
            public Task<ResultadoAsignacion> AceptarAsync(int usuarioId, int viajeId, int envioId) => throw new NotImplementedException();
            public Task<EvaluacionOportunidad> EvaluarCompatibilidadAsync(Viaje viaje, Envio envio, int usuarioId, DateOnly hoy) => throw new NotImplementedException();
        }

        private class OptimizadorServicioFalso : IOptimizadorServicio
        {
            // Envíos de la combinación óptima de cada viaje.
            public Dictionary<int, List<OportunidadDto>> PorViaje { get; } = new Dictionary<int, List<OportunidadDto>>();

            public Task<OptimizacionResultadoDto?> OptimizarAsync(int usuarioId, int viajeId, string? criterio = "ganancia")
            {
                var envios = PorViaje.GetValueOrDefault(viajeId, new List<OportunidadDto>());
                return Task.FromResult<OptimizacionResultadoDto?>(new OptimizacionResultadoDto
                {
                    ViajeId = viajeId,
                    Optima = new CombinacionDto { Envios = envios, GananciaTotal = envios.Sum(e => e.MontoPago) }
                });
            }
        }
    }
}
