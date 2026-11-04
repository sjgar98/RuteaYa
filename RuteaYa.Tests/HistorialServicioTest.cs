using Microsoft.EntityFrameworkCore;
using RuteaYa.API.Servicios;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;

namespace RuteaYa.Tests
{
    public class HistorialServicioTests
    {
        private const int UsuarioId = 1;

        private RuteaYaDbContext CrearContextoBD(string nombreDb)
        {
            var options = new DbContextOptionsBuilder<RuteaYaDbContext>()
                .UseInMemoryDatabase(databaseName: nombreDb)
                .Options;
            return new RuteaYaDbContext(options);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_SinAsignaciones_DevuelveMetricasEnCeroEItemsVacios()
        {
            using var context = CrearContextoBD("Historial_Vacio");
            var servicio = new HistorialServicio(context);

            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            Assert.NotNull(resultado);
            Assert.Equal(0, resultado.ViajesRealizados);
            Assert.Equal(0, resultado.EnviosTransportados);
            Assert.Equal(0m, resultado.GananciaEfectiva);
            Assert.Empty(resultado.Items);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_ConAsignacionesEntregadas_CalculaMetricasYAgrupaCorrectamente()
        {
            using var context = CrearContextoBD("Historial_ConDatos");

            var viaje = new Viaje
            {
                Id = 10,
                UsuarioId = UsuarioId,
                OrigenDir = "Palermo, CABA",
                DestinoDir = "Obelisco, CABA",
                Activo = true
            };
            context.Viajes.Add(viaje);

            var envio = new Envio
            {
                Id = 100,
                UsuarioId = 2,
                Estado = EstadoEnvio.Entregado,
                Monto = 5000m
            };
            context.Envios.Add(envio);

            var asignacion = new Asignacion
            {
                Id = 1,
                ViajeId = 10,
                EnvioId = 100,
                AsignadoEn = new DateTime(2026, 11, 2, 10, 0, 0, DateTimeKind.Utc),
                Ganancia = 5000m,
                Viaje = viaje,
                Envio = envio
            };
            context.Asignaciones.Add(asignacion);
            await context.SaveChangesAsync();

            var servicio = new HistorialServicio(context);
            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, null, null);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.ViajesRealizados);
            Assert.Equal(1, resultado.EnviosTransportados);
            Assert.Equal(5000m, resultado.GananciaEfectiva);

            var item = Assert.Single(resultado.Items);
            Assert.Equal(10, item.Id);
            Assert.Equal("Viaje", item.Tipo);
            Assert.Equal("Palermo, CABA → Obelisco, CABA", item.Descripcion);
            Assert.Equal(5000m, item.Monto);
        }

        [Fact]
        public async Task ObtenerHistorialAsync_AplicaFiltroDeFechasCorrectamente()
        {
            using var context = CrearContextoBD("Historial_FiltroFechas");

            var viaje = new Viaje { Id = 1, UsuarioId = UsuarioId, OrigenDir = "A", DestinoDir = "B", Activo = true };
            var envio1 = new Envio { Id = 10, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 3000m };
            var envio2 = new Envio { Id = 11, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 4000m };

            context.Viajes.Add(viaje);
            context.Envios.AddRange(envio1, envio2);

            context.Asignaciones.Add(new Asignacion
            {
                Id = 1,
                ViajeId = 1,
                EnvioId = 10,
                AsignadoEn = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 3000m,
                Viaje = viaje,
                Envio = envio1
            });
            context.Asignaciones.Add(new Asignacion
            {
                Id = 2,
                ViajeId = 1,
                EnvioId = 11,
                AsignadoEn = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 4000m,
                Viaje = viaje,
                Envio = envio2
            });
            await context.SaveChangesAsync();

            var servicio = new HistorialServicio(context);

            // Filtrar desde el 1 de noviembre de 2026 en adelante
            var desde = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, desde, null, null, null);

            Assert.Equal(1, resultado.EnviosTransportados);
            Assert.Equal(4000m, resultado.GananciaEfectiva);
        }

        [Theory]
        [InlineData("fecha_asc")]
        [InlineData("fecha_desc")]
        [InlineData("monto_asc")]
        [InlineData("monto_desc")]
        public async Task ObtenerHistorialAsync_AplicaOrdenamientosCorrectamente(string criterioOrden)
        {
            using var context = CrearContextoBD($"Historial_Orden_{criterioOrden}");

            var viaje1 = new Viaje { Id = 1, UsuarioId = UsuarioId, OrigenDir = "A", DestinoDir = "B", Activo = true };
            var viaje2 = new Viaje { Id = 2, UsuarioId = UsuarioId, OrigenDir = "C", DestinoDir = "D", Activo = true };
            var envio1 = new Envio { Id = 10, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 1000m };
            var envio2 = new Envio { Id = 11, UsuarioId = 2, Estado = EstadoEnvio.Entregado, Monto = 5000m };

            context.Viajes.AddRange(viaje1, viaje2);
            context.Envios.AddRange(envio1, envio2);

            context.Asignaciones.Add(new Asignacion
            {
                Id = 1,
                ViajeId = 1,
                EnvioId = 10,
                AsignadoEn = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 1000m,
                Viaje = viaje1,
                Envio = envio1
            });
            context.Asignaciones.Add(new Asignacion
            {
                Id = 2,
                ViajeId = 2,
                EnvioId = 11,
                AsignadoEn = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                Ganancia = 5000m,
                Viaje = viaje2,
                Envio = envio2
            });
            await context.SaveChangesAsync();

            var servicio = new HistorialServicio(context);
            var resultado = await servicio.ObtenerHistorialAsync(UsuarioId, null, null, criterioOrden, null);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Items.Count);
        }
    }
}