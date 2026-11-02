using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.API.Servicios.Rutas;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Rutas;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class OportunidadesServicioTests
    {
        private const int ConductorId = 1;
        private const int RemitenteId = 2;
        private const int ViajeId = 10;

        // Belgrano → Lomas de Zamora. Los envíos se ubican sobre esa recta para pasar el prefiltro.
        private const decimal OrigenLat = -34.5627m, OrigenLng = -58.4583m;
        private const decimal DestinoLat = -34.7608m, DestinoLng = -58.4060m;

        private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));

        private readonly ViajeRepositorioFalso _viajes = new ViajeRepositorioFalso();
        private readonly EnvioRepositorioFalso _envios = new EnvioRepositorioFalso();
        private readonly AsignacionRepositorioFalso _asignaciones;
        private readonly RutaServicioFalso _rutas;

        public OportunidadesServicioTests()
        {
            _rutas = new RutaServicioFalso(_envios);
            _asignaciones = new AsignacionRepositorioFalso(_viajes);
        }

        private OportunidadesServicio CrearServicio()
        {
            return new OportunidadesServicio(_viajes, _envios, _asignaciones, _rutas, NullLogger<OportunidadesServicio>.Instance);
        }

        // ---------- Listar ----------

        [Fact]
        public async Task ListarAsync_ViajeDeOtroUsuario_DevuelveNull()
        {
            _viajes.Viajes.Add(CrearViaje(usuarioId: 99));
            _envios.Envios.Add(CrearEnvio(1));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ListarAsync_ViajeInexistente_DevuelveNull()
        {
            var resultado = await CrearServicio().ListarAsync(ConductorId, 999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ListarAsync_ViajeInactivo_DevuelveElViajeSinOportunidades()
        {
            var viaje = CrearViaje();
            viaje.Activo = false;
            _viajes.Viajes.Add(viaje);
            _envios.Envios.Add(CrearEnvio(1));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.NotNull(resultado);
            Assert.False(resultado!.Activo);
            Assert.Empty(resultado.Oportunidades);
        }

        [Fact]
        public async Task ListarAsync_DevuelveDatosDelViajeYDeLaOportunidad()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, monto: 7000m, pesoKg: 8m));
            _rutas.Desvios[1] = 3m;

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.Equal("Belgrano, CABA", resultado!.OrigenDir);
            Assert.Equal(5m, resultado.DesvioMaxKm);
            var oportunidad = Assert.Single(resultado.Oportunidades);
            Assert.Equal(1, oportunidad.EnvioId);
            Assert.Equal(7000m, oportunidad.MontoPago);
            Assert.Equal(8m, oportunidad.PesoKg);
            Assert.Equal(3m, oportunidad.DesvioKm);
            Assert.Equal(Hoy, oportunidad.FechaViaje);
            Assert.Equal(3, oportunidad.CapacidadRestantePaquetes);
            Assert.Equal(10m, oportunidad.CapacidadRestanteKg);
        }

        [Fact]
        public async Task ListarAsync_NoOfreceEnviosPropiosNiNoDisponibles()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, usuarioId: ConductorId));
            _envios.Envios.Add(CrearEnvio(2, estado: EstadoEnvio.Cancelado));
            _envios.Envios.Add(CrearEnvio(3, estado: EstadoEnvio.ConductorAsignado));
            _envios.Envios.Add(CrearEnvio(4));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.Equal(new[] { 4 }, resultado!.Oportunidades.Select(o => o.EnvioId));
        }

        [Fact]
        public async Task ListarAsync_DescartaEnviosQueSuperanElDesvioMaximo()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1));
            _envios.Envios.Add(CrearEnvio(2));
            _rutas.Desvios[1] = 5m;
            _rutas.Desvios[2] = 5.1m;

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.Equal(new[] { 1 }, resultado!.Oportunidades.Select(o => o.EnvioId));
        }

        [Fact]
        public async Task ListarAsync_DescartaEnviosQueNoEntranPorPeso()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 10m));
            _envios.Envios.Add(CrearEnvio(2, pesoKg: 10.5m));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.Equal(new[] { 1 }, resultado!.Oportunidades.Select(o => o.EnvioId));
        }

        [Fact]
        public async Task ListarAsync_SoloOfreceEnviosConUnaSalidaDelViajeAntesDeLaFechaLimite()
        {
            // El viaje sale solo dentro de 3 días.
            var salida = Hoy.AddDays(3);
            _viajes.Viajes.Add(CrearViaje(dias: new[] { salida.ADiaSemana() }));
            _envios.Envios.Add(CrearEnvio(1, fechaLimite: Hoy.AddDays(2)));
            _envios.Envios.Add(CrearEnvio(2, fechaLimite: salida));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            var oportunidad = Assert.Single(resultado!.Oportunidades);
            Assert.Equal(2, oportunidad.EnvioId);
            Assert.Equal(salida, oportunidad.FechaViaje);
        }

        [Fact]
        public async Task ListarAsync_ViajeConTodosLosPaquetesAceptados_NoOfreceMasAunqueSalgaOtrosDias()
        {
            // El límite es del viaje: aunque el envío aceptado sea para hoy, mañana tampoco hay lugar.
            var viaje = CrearViaje(maxPaquetes: 1);
            viaje.Asignaciones.Add(CrearAsignacion(Hoy, CrearEnvio(50, estado: EstadoEnvio.ConductorAsignado)));
            _viajes.Viajes.Add(viaje);
            _envios.Envios.Add(CrearEnvio(1, fechaLimite: Hoy.AddDays(5)));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.Empty(resultado!.Oportunidades);
            Assert.Equal(0, resultado.PaquetesDisponibles);
        }

        [Fact]
        public async Task ListarAsync_DescartaEnviosQueSuperanElPesoLibreContandoLosYaAceptados()
        {
            // 10 kg de máximo y 6 kg ya aceptados (en otra fecha): quedan 4 kg.
            var viaje = CrearViaje(pesoMaxKg: 10m);
            viaje.Asignaciones.Add(CrearAsignacion(Hoy.AddDays(3), CrearEnvio(50, estado: EstadoEnvio.Retirado, pesoKg: 6m)));
            _viajes.Viajes.Add(viaje);
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 4m));
            _envios.Envios.Add(CrearEnvio(2, pesoKg: 4.5m));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            var oportunidad = Assert.Single(resultado!.Oportunidades);
            Assert.Equal(1, oportunidad.EnvioId);
            Assert.Equal(4m, resultado.KgDisponibles);
            Assert.Equal(2, resultado.PaquetesDisponibles);
            Assert.Equal(4m, oportunidad.CapacidadRestanteKg);
        }

        [Fact]
        public async Task ListarAsync_EnviosEntregadosYCanceladosLiberanLugar()
        {
            var viaje = CrearViaje(maxPaquetes: 1, pesoMaxKg: 5m);
            viaje.Asignaciones.Add(CrearAsignacion(Hoy.AddDays(-7), CrearEnvio(50, estado: EstadoEnvio.Entregado, pesoKg: 5m)));
            viaje.Asignaciones.Add(CrearAsignacion(Hoy, CrearEnvio(51, estado: EstadoEnvio.Cancelado, pesoKg: 5m)));
            _viajes.Viajes.Add(viaje);
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 5m));

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            var oportunidad = Assert.Single(resultado!.Oportunidades);
            Assert.Equal(Hoy, oportunidad.FechaViaje);
            Assert.Equal(1, resultado.PaquetesDisponibles);
        }

        [Fact]
        public async Task ListarAsync_EtiquetaLaCompatibilidadSegunElDesvioRespectoDelMaximo()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1));
            _envios.Envios.Add(CrearEnvio(2));
            _envios.Envios.Add(CrearEnvio(3));
            _rutas.Desvios[1] = 1m;
            _rutas.Desvios[2] = 2m;
            _rutas.Desvios[3] = 3m;

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            var porId = resultado!.Oportunidades.ToDictionary(o => o.EnvioId);
            Assert.Equal("Muy compatible", porId[1].Compatibilidad);
            Assert.Equal("Compatible", porId[2].Compatibilidad);
            Assert.Equal("Mucho desvío", porId[3].Compatibilidad);
            Assert.Equal(0.8m, porId[1].PuntuacionCompatibilidad);
        }

        [Fact]
        public async Task ListarAsync_PorGanancia_OrdenaDeMayorAMenorYDesempataPorMenorDesvio()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, monto: 4000m));
            _envios.Envios.Add(CrearEnvio(2, monto: 7000m));
            _envios.Envios.Add(CrearEnvio(3, monto: 4000m));
            _rutas.Desvios[1] = 2m;
            _rutas.Desvios[3] = 1m;

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId, "ganancia");

            Assert.Equal(new[] { 2, 3, 1 }, resultado!.Oportunidades.Select(o => o.EnvioId));
        }

        [Fact]
        public async Task ListarAsync_PorDesvio_OrdenaDeMenorAMayor()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1));
            _envios.Envios.Add(CrearEnvio(2));
            _envios.Envios.Add(CrearEnvio(3));
            _rutas.Desvios[1] = 3m;
            _rutas.Desvios[2] = 0.5m;
            _rutas.Desvios[3] = 2m;

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId, "desvio");

            Assert.Equal(new[] { 2, 3, 1 }, resultado!.Oportunidades.Select(o => o.EnvioId));
        }

        [Fact]
        public async Task ListarAsync_SiFallaLaRutaDeUnEnvio_DescartaSoloEseEnvio()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1));
            _envios.Envios.Add(CrearEnvio(2));
            _rutas.EnviosQueFallan.Add(1);

            var resultado = await CrearServicio().ListarAsync(ConductorId, ViajeId);

            Assert.Equal(new[] { 2 }, resultado!.Oportunidades.Select(o => o.EnvioId));
        }

        // ---------- Detalle ----------

        [Fact]
        public async Task ObtenerDetalleAsync_CompletaRutaPagoYCapacidadRestanteDespuesDeAceptar()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, monto: 7000m, pesoKg: 8m));
            _rutas.Desvios[1] = 3m;

            var detalle = await CrearServicio().ObtenerDetalleAsync(ConductorId, ViajeId, 1);

            Assert.NotNull(detalle);
            Assert.True(detalle!.PuedeAceptar);
            Assert.Null(detalle.Motivo);
            Assert.Equal(Hoy, detalle.FechaViaje);
            Assert.Equal("Mucho desvío", detalle.Compatibilidad);
            Assert.Equal(3m, detalle.RutaDetalle.DesvioKm);
            Assert.Equal(7000m, detalle.RutaDetalle.MontoPago);
            Assert.Equal(2333.33m, detalle.RutaDetalle.GananciaPorKmAdicional);
            Assert.Equal(2, detalle.RutaDetalle.CapacidadRestantePaquetes);
            Assert.Equal(2m, detalle.RutaDetalle.CapacidadRestanteKg);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_ViajeDeOtroUsuario_DevuelveNull()
        {
            _viajes.Viajes.Add(CrearViaje(usuarioId: 99));
            _envios.Envios.Add(CrearEnvio(1));

            Assert.Null(await CrearServicio().ObtenerDetalleAsync(ConductorId, ViajeId, 1));
        }

        [Theory]
        [InlineData(EstadoEnvio.ConductorAsignado)]
        [InlineData(EstadoEnvio.Cancelado)]
        [InlineData(EstadoEnvio.Entregado)]
        public async Task ObtenerDetalleAsync_EnvioQueYaNoBuscaConductor_DevuelveNull(EstadoEnvio estado)
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, estado: estado));

            Assert.Null(await CrearServicio().ObtenerDetalleAsync(ConductorId, ViajeId, 1));
        }

        [Fact]
        public async Task ObtenerDetalleAsync_EnvioPropio_DevuelveNull()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, usuarioId: ConductorId));

            Assert.Null(await CrearServicio().ObtenerDetalleAsync(ConductorId, ViajeId, 1));
        }

        [Fact]
        public async Task ObtenerDetalleAsync_SiYaNoSeCumplenLasRestricciones_NoPermiteAceptarYExplicaElMotivo()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1));
            _rutas.Desvios[1] = 7m;

            var detalle = await CrearServicio().ObtenerDetalleAsync(ConductorId, ViajeId, 1);

            Assert.False(detalle!.PuedeAceptar);
            Assert.Contains("desvío", detalle.Motivo);
            Assert.Equal(7m, detalle.RutaDetalle.DesvioKm);
        }

        // ---------- Aceptar ----------

        [Fact]
        public async Task AceptarAsync_CreaLaAsignacionYPasaElEnvioAConductorAsignado()
        {
            _viajes.Viajes.Add(CrearViaje());
            var envio = CrearEnvio(1, monto: 5500m);
            _envios.Envios.Add(envio);
            _rutas.Desvios[1] = 2m;

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Exito, resultado.Resultado);
            var asignacion = Assert.Single(_asignaciones.Agregadas);
            Assert.Equal(ViajeId, asignacion.ViajeId);
            Assert.Equal(1, asignacion.EnvioId);
            Assert.Equal(Hoy, asignacion.FechaViaje);
            Assert.Equal(2m, asignacion.DesvioKm);
            Assert.Equal(5500m, asignacion.Ganancia);
            Assert.Equal(DateTimeKind.Utc, asignacion.AsignadoEn.Kind);

            Assert.Equal(EstadoEnvio.ConductorAsignado, envio.Estado);
            var historial = Assert.Single(envio.Historial);
            Assert.Equal(EstadoEnvio.BuscandoConductor, historial.EstadoAnterior);
            Assert.Equal(EstadoEnvio.ConductorAsignado, historial.EstadoNuevo);

            Assert.Equal(Hoy, resultado.Asignacion!.FechaViaje);
        }

        [Fact]
        public async Task AceptarAsync_ViajeDeOtroUsuario_DevuelveNoEncontradoYNoAsigna()
        {
            _viajes.Viajes.Add(CrearViaje(usuarioId: 99));
            var envio = CrearEnvio(1);
            _envios.Envios.Add(envio);

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado.Resultado);
            Assert.Empty(_asignaciones.Agregadas);
            Assert.Equal(EstadoEnvio.BuscandoConductor, envio.Estado);
        }

        [Fact]
        public async Task AceptarAsync_EnvioInexistente_DevuelveNoEncontrado()
        {
            _viajes.Viajes.Add(CrearViaje());

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 999);

            Assert.Equal(ResultadoOperacion.NoEncontrado, resultado.Resultado);
        }

        [Theory]
        [InlineData(EstadoEnvio.ConductorAsignado)]
        [InlineData(EstadoEnvio.Cancelado)]
        public async Task AceptarAsync_EnvioQueYaNoBuscaConductor_DevuelveConflicto(EstadoEnvio estado)
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, estado: estado));

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado.Resultado);
            Assert.Empty(_asignaciones.Agregadas);
        }

        [Fact]
        public async Task AceptarAsync_EnvioPropio_DevuelveConflicto()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1, usuarioId: ConductorId));

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado.Resultado);
            Assert.Empty(_asignaciones.Agregadas);
        }

        [Fact]
        public async Task AceptarAsync_ViajeInactivo_DevuelveConflicto()
        {
            var viaje = CrearViaje();
            viaje.Activo = false;
            _viajes.Viajes.Add(viaje);
            _envios.Envios.Add(CrearEnvio(1));

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado.Resultado);
            Assert.Contains("inactivo", resultado.Mensaje);
        }

        [Fact]
        public async Task AceptarAsync_ViajeConTodosLosPaquetesAceptados_DevuelveConflictoYNoCambiaElEstado()
        {
            var viaje = CrearViaje(maxPaquetes: 1);
            viaje.Asignaciones.Add(CrearAsignacion(Hoy.AddDays(2), CrearEnvio(50, estado: EstadoEnvio.ConductorAsignado)));
            _viajes.Viajes.Add(viaje);
            var envio = CrearEnvio(1, fechaLimite: Hoy.AddDays(10));
            _envios.Envios.Add(envio);

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado.Resultado);
            Assert.Contains("1 de 1 paquetes", resultado.Mensaje);
            Assert.Equal(EstadoEnvio.BuscandoConductor, envio.Estado);
            Assert.Empty(envio.Historial);
        }

        [Fact]
        public async Task AceptarAsync_SuperaElPesoMaximoContandoLosYaAceptados_DevuelveConflicto()
        {
            var viaje = CrearViaje(pesoMaxKg: 10m);
            viaje.Asignaciones.Add(CrearAsignacion(Hoy.AddDays(2), CrearEnvio(50, estado: EstadoEnvio.EnCamino, pesoKg: 7m)));
            _viajes.Viajes.Add(viaje);
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 4m));

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado.Resultado);
            Assert.Contains("11 kg", resultado.Mensaje);
            Assert.Empty(_asignaciones.Agregadas);
        }

        [Fact]
        public async Task AceptarAsync_UnoTrasOtro_NoPermitePasarseDeLaCantidadDePaquetes()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 2));
            _envios.Envios.Add(CrearEnvio(1));
            _envios.Envios.Add(CrearEnvio(2));
            _envios.Envios.Add(CrearEnvio(3));
            var servicio = CrearServicio();

            Assert.Equal(ResultadoOperacion.Exito, (await servicio.AceptarAsync(ConductorId, ViajeId, 1)).Resultado);
            Assert.Equal(ResultadoOperacion.Exito, (await servicio.AceptarAsync(ConductorId, ViajeId, 2)).Resultado);
            var tercero = await servicio.AceptarAsync(ConductorId, ViajeId, 3);

            Assert.Equal(ResultadoOperacion.Conflicto, tercero.Resultado);
            Assert.Equal(2, _asignaciones.Agregadas.Count);
        }

        [Fact]
        public async Task AceptarAsync_UnoTrasOtro_NoPermitePasarseDelPesoMaximo()
        {
            _viajes.Viajes.Add(CrearViaje(maxPaquetes: 5, pesoMaxKg: 10m));
            _envios.Envios.Add(CrearEnvio(1, pesoKg: 6m));
            _envios.Envios.Add(CrearEnvio(2, pesoKg: 4m));
            _envios.Envios.Add(CrearEnvio(3, pesoKg: 0.5m));
            var servicio = CrearServicio();

            Assert.Equal(ResultadoOperacion.Exito, (await servicio.AceptarAsync(ConductorId, ViajeId, 1)).Resultado);
            Assert.Equal(ResultadoOperacion.Exito, (await servicio.AceptarAsync(ConductorId, ViajeId, 2)).Resultado);
            var tercero = await servicio.AceptarAsync(ConductorId, ViajeId, 3);

            Assert.Equal(ResultadoOperacion.Conflicto, tercero.Resultado);
            Assert.Equal(10m, _asignaciones.Agregadas.Sum(a => a.Envio.PesoKg));
        }

        [Fact]
        public async Task AceptarAsync_DesvioMayorAlMaximo_DevuelveConflicto()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1));
            _rutas.Desvios[1] = 6m;

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado.Resultado);
            Assert.Contains("desvío", resultado.Mensaje);
        }

        [Fact]
        public async Task AceptarAsync_OtroConductorLoTomoAlMismoTiempo_DevuelveConflicto()
        {
            _viajes.Viajes.Add(CrearViaje());
            _envios.Envios.Add(CrearEnvio(1));
            _asignaciones.FallarPorIndiceUnico = true;

            var resultado = await CrearServicio().AceptarAsync(ConductorId, ViajeId, 1);

            Assert.Equal(ResultadoOperacion.Conflicto, resultado.Resultado);
            Assert.Contains("Otro conductor", resultado.Mensaje);
        }

        // ---------- Datos de prueba ----------

        private static Viaje CrearViaje(int usuarioId = ConductorId, int maxPaquetes = 3, decimal pesoMaxKg = 10m,
            decimal desvioMaxKm = 5m, DiaSemana[]? dias = null)
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

            foreach (var dia in dias ?? Enum.GetValues<DiaSemana>())
            {
                viaje.Dias.Add(new ViajeDia { ViajeId = ViajeId, DiaSemana = dia });
            }

            return viaje;
        }

        private static Envio CrearEnvio(int id, int usuarioId = RemitenteId, EstadoEnvio estado = EstadoEnvio.BuscandoConductor,
            decimal monto = 4000m, decimal pesoKg = 2m, DateOnly? fechaLimite = null)
        {
            // Retiro y entrega sobre la recta del viaje (así el prefiltro en línea recta no los descarta);
            // el retiro se corre apenas según el id para que el servicio de rutas falso identifique el envío.
            var fraccionRetiro = 0.2m + id / 10000m;
            return new Envio
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = $"Retiro {id}",
                OrigenLat = OrigenLat + (DestinoLat - OrigenLat) * fraccionRetiro,
                OrigenLng = OrigenLng + (DestinoLng - OrigenLng) * fraccionRetiro,
                DestinoDir = $"Entrega {id}",
                DestinoLat = OrigenLat + (DestinoLat - OrigenLat) * 0.75m,
                DestinoLng = OrigenLng + (DestinoLng - OrigenLng) * 0.75m,
                FechaLimite = fechaLimite ?? Hoy.AddDays(14),
                PesoKg = pesoKg,
                Tamano = TamanoEnvio.Pequeno,
                Descripcion = $"Paquete {id}",
                Monto = monto,
                Estado = estado
            };
        }

        private static Asignacion CrearAsignacion(DateOnly fecha, Envio envio)
        {
            return new Asignacion { ViajeId = ViajeId, EnvioId = envio.Id, FechaViaje = fecha, Envio = envio };
        }

        private class ViajeRepositorioFalso : IViajeRepositorio
        {
            public List<Viaje> Viajes { get; } = new List<Viaje>();

            public Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null)
            {
                return Task.FromResult(Viajes.Where(v => v.UsuarioId == usuarioId && (activo == null || v.Activo == activo)).ToList());
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

        private class EnvioRepositorioFalso : IEnvioRepositorio
        {
            public List<Envio> Envios { get; } = new List<Envio>();

            public Task<List<Envio>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null)
            {
                return Task.FromResult(Envios.Where(e => e.UsuarioId == usuarioId && (estado == null || e.Estado == estado)).ToList());
            }

            // Mismo filtro que EnvioRepositorio.ListarDisponiblesAsync.
            public Task<List<Envio>> ListarDisponiblesAsync(DateOnly desde, int usuarioIdExcluido)
            {
                return Task.FromResult(Envios
                    .Where(e => e.Estado == EstadoEnvio.BuscandoConductor && e.FechaLimite >= desde && e.UsuarioId != usuarioIdExcluido)
                    .OrderBy(e => e.FechaLimite)
                    .ToList());
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
                return Task.CompletedTask;
            }

            public Task GuardarCambiosAsync()
            {
                return Task.CompletedTask;
            }
        }

        private class AsignacionRepositorioFalso : IAsignacionRepositorio
        {
            private readonly ViajeRepositorioFalso _viajes;

            public AsignacionRepositorioFalso(ViajeRepositorioFalso viajes)
            {
                _viajes = viajes;
            }

            public List<Asignacion> Agregadas { get; } = new List<Asignacion>();
            public bool FallarPorIndiceUnico { get; set; }

            public Task<List<Asignacion>> ListarPorConductorAsync(int conductorId)
            {
                return Task.FromResult(Agregadas.ToList());
            }

            public Task<Asignacion?> ObtenerPorIdAsync(int id)
            {
                return Task.FromResult(Agregadas.FirstOrDefault(a => a.Id == id));
            }

            public Task AgregarAsync(List<Asignacion> asignaciones)
            {
                if (FallarPorIndiceUnico)
                {
                    throw new DbUpdateException("Índice único IX_Asignaciones_EnvioId");
                }

                foreach (var asignacion in asignaciones)
                {
                    asignacion.Id = Agregadas.Count + 1;
                    Agregadas.Add(asignacion);
                    // Como la base: la próxima vez que se lea el viaje, la asignación ya está.
                    _viajes.Viajes.Single(v => v.Id == asignacion.ViajeId).Asignaciones.Add(asignacion);
                }

                return Task.CompletedTask;
            }

            public Task GuardarCambiosAsync()
            {
                return Task.CompletedTask;
            }
        }

        // Devuelve el desvío configurado para cada envío (1 km por defecto), que identifica por su retiro.
        private class RutaServicioFalso : IRutaServicio
        {
            private readonly EnvioRepositorioFalso _envios;

            public RutaServicioFalso(EnvioRepositorioFalso envios)
            {
                _envios = envios;
            }

            public Dictionary<int, decimal> Desvios { get; } = new Dictionary<int, decimal>();
            public HashSet<int> EnviosQueFallan { get; } = new HashSet<int>();

            public Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(IReadOnlyList<CoordenadaDto> recorridoHabitual,
                CoordenadaDto retiro, CoordenadaDto entrega, CancellationToken cancelacion = default)
            {
                var envio = _envios.Envios.Single(e => e.OrigenLat == retiro.Latitud && e.OrigenLng == retiro.Longitud);
                if (EnviosQueFallan.Contains(envio.Id))
                {
                    throw new RutaNoDisponibleException("Ruta no encontrada.");
                }

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
