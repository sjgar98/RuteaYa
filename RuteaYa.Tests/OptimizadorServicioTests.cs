using Microsoft.Extensions.Logging.Abstractions;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Rutas;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    // Tests del OptimizadorServicio con los repositorios y servicios falsos.
    // Cubre: el ejemplo del enunciado (sección 12), empates, casos límite y sin candidatos.
    public class OptimizadorServicioTests
    {
        private const int ConductorId = 1;
        private const int RemitenteId = 2;
        private const int ViajeId = 10;

        // Belgrano → Lomas de Zamora (mismas coordenadas que OportunidadesServicioTests).
        private const decimal OrigenLat = -34.5627m, OrigenLng = -58.4583m;
        private const decimal DestinoLat = -34.7608m, DestinoLng = -58.4060m;

        private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));

        private readonly ViajeRepositorioFalso _viajes = new ViajeRepositorioFalso();
        private readonly EnvioRepositorioFalso _envios = new EnvioRepositorioFalso();
        private readonly AsignacionRepositorioFalso _asignaciones = new AsignacionRepositorioFalso();
        private readonly RutaServicioFalso _rutas;

        public OptimizadorServicioTests()
        {
            _rutas = new RutaServicioFalso(_envios);
        }

        private OptimizadorServicio CrearServicio()
        {
            var oportunidades = new OportunidadesServicio(
                _viajes, _envios, _asignaciones, _rutas,
                NullLogger<OportunidadesServicio>.Instance);

            return new OptimizadorServicio(
                _viajes, _envios, oportunidades,
                NullLogger<OptimizadorServicio>.Instance);
        }

        // ---------- Sin candidatos / sin viaje ----------

        [Fact]
        public async Task OptimizarAsync_ViajeInexistente_DevuelveNull()
        {
            var resultado = await CrearServicio().OptimizarAsync(ConductorId, 999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task OptimizarAsync_ViajeDeOtroUsuario_DevuelveNull()
        {
            _viajes.Viajes.Add(CrearViaje(usuarioId: 99));

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task OptimizarAsync_SinEnviosDisponibles_DevuelveOptimaVacia()
        {
            _viajes.Viajes.Add(CrearViaje());

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId);

            Assert.NotNull(resultado);
            Assert.Empty(resultado!.Optima.Envios);
            Assert.Empty(resultado.OtrasCombinaciones);
            Assert.Empty(resultado.Descartados);
        }

        [Fact]
        public async Task OptimizarAsync_TodosDescartados_DevuelveOptimaVaciaYDescartados()
        {
            _viajes.Viajes.Add(CrearViaje(pesoMaxKg: 1m)); // Peso máximo 1 kg
            // Envío de 5 kg: no entra por peso.
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 5m));

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId);

            Assert.NotNull(resultado);
            Assert.Empty(resultado!.Optima.Envios);
            Assert.Single(resultado.Descartados);
        }

        // ---------- Ejemplo del enunciado (sección 12) ----------
        // Envíos: A=$4000/2kg/1km, B=$5500/6kg/2km, C=$3500/4kg/1km, D=$7000/8kg/3km, E=$2500/1kg/0.5km
        // Restricciones: 3 paquetes, 10 kg, 5 km desvío.
        // Resultado esperado: A+B+E = $12.000 (mejor ganancia que cumple todo).

        [Fact]
        public async Task OptimizarAsync_EjemploEnunciado_EncuentraLaCombinacionOptima()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 3, pesoMaxKg: 10m, desvioMaxKm: 5m));

            var envioA = CrearEnvio(1, monto: 4000m, pesoKg: 2m);
            var envioB = CrearEnvio(2, monto: 5500m, pesoKg: 6m);
            var envioC = CrearEnvio(3, monto: 3500m, pesoKg: 4m);
            var envioD = CrearEnvio(4, monto: 7000m, pesoKg: 8m);
            var envioE = CrearEnvio(5, monto: 2500m, pesoKg: 1m);

            _rutas.Desvios[1] = 1.0m;
            _rutas.Desvios[2] = 2.0m;
            _rutas.Desvios[3] = 1.0m;
            _rutas.Desvios[4] = 3.0m;
            _rutas.Desvios[5] = 0.5m;

            _envios.Envios.AddRange(new[] { envioA, envioB, envioC, envioD, envioE });

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId, "ganancia");

            Assert.NotNull(resultado);
            var optima = resultado!.Optima;

            // Combinación óptima: A+B+E = $12.000, 9 kg, 3.5 km
            Assert.Equal(12000m, optima.GananciaTotal);
            Assert.Equal(3, optima.CantidadPaquetes);
            Assert.Equal(9m, optima.PesoTotalKg);
            Assert.Equal(3.5m, optima.DesvioTotalKm);

            var idsOptima = optima.Envios.Select(e => e.EnvioId).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 2, 5 }, idsOptima);
        }

        [Fact]
        public async Task OptimizarAsync_EjemploEnunciado_DevolverOtrasCombinacionesValidas()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 3, pesoMaxKg: 10m, desvioMaxKm: 5m));

            _rutas.Desvios[1] = 1.0m;
            _rutas.Desvios[2] = 2.0m;
            _rutas.Desvios[3] = 1.0m;
            _rutas.Desvios[4] = 3.0m;
            _rutas.Desvios[5] = 0.5m;

            _envios.Envios.AddRange(new[]
            {
                CrearEnvio(1, monto: 4000m, pesoKg: 2m),
                CrearEnvio(2, monto: 5500m, pesoKg: 6m),
                CrearEnvio(3, monto: 3500m, pesoKg: 4m),
                CrearEnvio(4, monto: 7000m, pesoKg: 8m),
                CrearEnvio(5, monto: 2500m, pesoKg: 1m)
            });

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId, "ganancia");

            // Debe haber más de una combinación válida además de la óptima.
            Assert.NotEmpty(resultado!.OtrasCombinaciones);
        }

        // ---------- Empates ----------

        [Fact]
        public async Task OptimizarAsync_EmpateGanancia_PrefiereMenorDesvio()
        {
            // Dos envíos con la misma ganancia pero distinto desvío.
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 1, pesoMaxKg: 10m, desvioMaxKm: 5m));

            var envio1 = CrearEnvio(1, monto: 5000m, pesoKg: 2m);
            var envio2 = CrearEnvio(2, monto: 5000m, pesoKg: 2m);

            _rutas.Desvios[1] = 3.0m; // Mayor desvío
            _rutas.Desvios[2] = 1.0m; // Menor desvío → debe ganar

            _envios.Envios.AddRange(new[] { envio1, envio2 });

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId, "ganancia");

            Assert.NotNull(resultado);
            Assert.Equal(5000m, resultado!.Optima.GananciaTotal);
            Assert.Equal(1.0m, resultado.Optima.DesvioTotalKm);
            Assert.Equal(2, resultado.Optima.Envios[0].EnvioId);
        }

        // ---------- Restricciones individuales ----------

        [Fact]
        public async Task OptimizarAsync_UnEnvioSuperaPeso_SeDescarta()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 3, pesoMaxKg: 5m, desvioMaxKm: 5m));
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 6m)); // supera el peso
            _rutas.Desvios[1] = 1.0m;

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId);

            Assert.Empty(resultado!.Optima.Envios);
            Assert.Single(resultado.Descartados);
        }

        [Fact]
        public async Task OptimizarAsync_UnEnvioSuperaDesvio_SeDescarta()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 3, pesoMaxKg: 10m, desvioMaxKm: 2m));
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 2m));
            _rutas.Desvios[1] = 3.0m; // supera el desvío máximo

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId);

            Assert.Empty(resultado!.Optima.Envios);
            Assert.Single(resultado.Descartados);
        }

        [Fact]
        public async Task OptimizarAsync_ViajeMaxUnPaquete_EligeElMejorIndividual()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 1, pesoMaxKg: 10m, desvioMaxKm: 5m));

            _rutas.Desvios[1] = 1.0m;
            _rutas.Desvios[2] = 2.0m;

            _envios.Envios.AddRange(new[]
            {
                CrearEnvio(1, monto: 3000m, pesoKg: 2m),
                CrearEnvio(2, monto: 5000m, pesoKg: 2m) // este es mejor
            });

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId, "ganancia");

            Assert.Equal(1, resultado!.Optima.CantidadPaquetes);
            Assert.Equal(5000m, resultado.Optima.GananciaTotal);
        }

        // ---------- Criterio desvío ----------

        [Fact]
        public async Task OptimizarAsync_CriterioDesvio_PriorizaMenorDesvio()
        {
            // Viaje con capacidad de 1 paquete.
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 1, pesoMaxKg: 10m, desvioMaxKm: 5m));

            _rutas.Desvios[1] = 0.5m; // menor desvío, menor ganancia
            _rutas.Desvios[2] = 2.0m; // mayor desvío, mayor ganancia

            _envios.Envios.AddRange(new[]
            {
                CrearEnvio(1, monto: 2000m, pesoKg: 2m),
                CrearEnvio(2, monto: 8000m, pesoKg: 2m)
            });

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId, "desvio");

            // Con criterio "desvio" debe elegir el de menor desvío aunque gane menos.
            Assert.Equal(0.5m, resultado!.Optima.DesvioTotalKm);
            Assert.Equal(1, resultado.Optima.Envios[0].EnvioId);
        }

        // ---------- Criterio equilibrio ----------

        [Fact]
        public async Task OptimizarAsync_CriterioEquilibrio_BalanceaGananciaYDesvio()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 1, pesoMaxKg: 10m, desvioMaxKm: 5m));

            // Envío 1: $1000 / 4km de desvío → equilibrio = 1000/5 = 200
            // Envío 2: $5000 / 0.5km de desvío → equilibrio = 5000/1.5 ≈ 3333 → debe ganar
            _rutas.Desvios[1] = 4.0m;
            _rutas.Desvios[2] = 0.5m;

            _envios.Envios.AddRange(new[]
            {
                CrearEnvio(1, monto: 1000m, pesoKg: 2m),
                CrearEnvio(2, monto: 5000m, pesoKg: 2m)
            });

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId, "equilibrio");

            Assert.Equal(2, resultado!.Optima.Envios[0].EnvioId);
        }

        // ---------- Envíos propios se excluyen ----------

        [Fact]
        public async Task OptimizarAsync_EnvioPropio_NoSeOfrece()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, usuarioId: ConductorId)); // es del mismo conductor
            _rutas.Desvios[1] = 1.0m;

            var resultado = await CrearServicio().OptimizarAsync(ConductorId, ViajeId);

            Assert.NotNull(resultado);
            Assert.Empty(resultado!.Optima.Envios);
            Assert.Empty(resultado.Descartados);
        }

        // ---------- Datos de prueba ----------

        private static Viaje CrearViaje(int usuarioId = ConductorId, int maxPaquetes = 3,
            decimal pesoMaxKg = 10m, decimal desvioMaxKm = 5m)
        {
            var viaje = new Viaje
            {
                Id = ViajeId,
                UsuarioId = usuarioId,
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
                DesvioMaxKm = desvioMaxKm,
                Activo = true
            };

            // Todos los días de la semana para que las fechas de los envíos caigan siempre.
            foreach (var dia in Enum.GetValues<DiaSemana>())
            {
                viaje.Dias.Add(new ViajeDia { ViajeId = ViajeId, DiaSemana = dia });
            }

            return viaje;
        }

        private static Envio CrearEnvio(int id, int usuarioId = RemitenteId,
            decimal monto = 4000m, decimal pesoKg = 2m, DateOnly? fechaLimite = null)
        {
            // Coordenadas sobre la recta del viaje para pasar el prefiltro de línea recta.
            var fraccion = 0.2m + id / 10000m;
            return new Envio
            {
                Id = id,
                UsuarioId = usuarioId,
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
            };
        }

        // ---------- Infraestructura de prueba (repositorios y servicio de rutas falsos) ----------

        private class ViajeRepositorioFalso : IViajeRepositorio
        {
            public List<Viaje> Viajes { get; } = new List<Viaje>();

            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null)
                => Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId && (activo == null || v.Activo == activo)).ToList());

            public Task<Viaje?> ObtenerPorIdAsync(int id)
                => Task.FromResult(Viajes.FirstOrDefault(v => v.Id == id));

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

            public Task<List<Envio>> ListarPorIdsAsync(List<int> ids)
                => Task.FromResult(Envios.Where(e => ids.Contains(e.Id)).ToList());

            public Task<Envio?> ObtenerPorIdAsync(int id)
                => Task.FromResult(Envios.FirstOrDefault(e => e.Id == id));

            public Task AgregarAsync(Envio envio) { Envios.Add(envio); return Task.CompletedTask; }
            public Task GuardarCambiosAsync() => Task.CompletedTask;
        }

        private class AsignacionRepositorioFalso : IAsignacionRepositorio
        {
            public Task<List<Asignacion>> ListarPorConductorAsync(int conductorId) => Task.FromResult(new List<Asignacion>());
            public Task<Asignacion?> ObtenerPorIdAsync(int id) => Task.FromResult<Asignacion?>(null);
            public Task AgregarAsync(List<Asignacion> asignaciones) => Task.CompletedTask;
            public Task GuardarCambiosAsync() => Task.CompletedTask;
        }

        // Devuelve el desvío configurado para cada envío (1 km por defecto), identificado por su retiro.
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
            public Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion) => throw new NotSupportedException();
            public Task<RutaResultadoDto> CalcularRutaConParadasAsync(List<string> paradas) => throw new NotSupportedException();
            public Task<RutaResultadoDto> CalcularRutaAlternativaEnvioAsync(string origen, string destino, string retiroPaquete, string entregaPaquete) => throw new NotSupportedException();
            public Task<decimal> CalcularDesvioAsync(string rutaOriginalId, string paradaExtra) => throw new NotSupportedException();
            public Task<DetalleOportunidadRutaDto> ObtenerDetalleOportunidadAsync(string origenHabitual, string destinoHabitual, string retiroEnvio, string entregaEnvio) => throw new NotSupportedException();
            public Task<List<string>> BuscarDireccionesAsync(string texto) => throw new NotSupportedException();
        }
    }
}

