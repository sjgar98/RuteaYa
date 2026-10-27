using Microsoft.EntityFrameworkCore;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;

namespace RuteaYa.API.Servicios
{
    public static class InicializadorDatos
    {
        public static async Task InicializarAsync(RuteaYaDbContext context)
        {
        
            if (await context.Viajes.AnyAsync())
            {
                return;
            }

            int usuarioId = 1; // Usuario por defecto (prueba@ruteaya.com)

            // --- 5 VIAJES DE PRUEBA (Inactivos para el Historial) ---
            var viaje1 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Belgrano", DestinoDir = "Lomas de Zamora", HoraSalida = new TimeOnly(8, 0), HoraLlegada = new TimeOnly(8, 55), MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 5m, DistanciaKm = 18.4m, DuracionMin = 55, Activo = false };
            var viaje2 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Palermo", DestinoDir = "San Isidro", HoraSalida = new TimeOnly(18, 30), HoraLlegada = new TimeOnly(19, 20), MaxPaquetes = 2, PesoMaxKg = 5m, DesvioMaxKm = 3m, DistanciaKm = 15.0m, DuracionMin = 50, Activo = false };
            var viaje3 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Avellaneda", DestinoDir = "Palermo", HoraSalida = new TimeOnly(9, 0), HoraLlegada = new TimeOnly(9, 40), MaxPaquetes = 4, PesoMaxKg = 12m, DesvioMaxKm = 4m, DistanciaKm = 12.0m, DuracionMin = 40, Activo = false };
            var viaje4 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Caballito", DestinoDir = "Lomas", HoraSalida = new TimeOnly(14, 0), HoraLlegada = new TimeOnly(14, 45), MaxPaquetes = 3, PesoMaxKg = 8m, DesvioMaxKm = 3m, DistanciaKm = 14.2m, DuracionMin = 45, Activo = false };
            var viaje5 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Morón", DestinoDir = "Lomas", HoraSalida = new TimeOnly(10, 0), HoraLlegada = new TimeOnly(11, 0), MaxPaquetes = 5, PesoMaxKg = 15m, DesvioMaxKm = 6m, DistanciaKm = 22.5m, DuracionMin = 60, Activo = false };

            context.Viajes.AddRange(viaje1, viaje2, viaje3, viaje4, viaje5);
            await context.SaveChangesAsync();

            // --- 5 ENVÍOS ENTREGADOS ---
            var envio1 = new Envio { UsuarioId = usuarioId, OrigenDir = "Plaza Italia", DestinoDir = "Obelisco", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 2m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Zapatillas", Monto = 4000m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-6) };
            var envio2 = new Envio { UsuarioId = usuarioId, OrigenDir = "Recoleta", DestinoDir = "Quilmes", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 4m, Tamano = TamanoEnvio.Mediano, Descripcion = "Documentos", Monto = 5500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-5) };
            var envio3 = new Envio { UsuarioId = usuarioId, OrigenDir = "Avellaneda", DestinoDir = "Palermo", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 3m, Tamano = TamanoEnvio.Mediano, Descripcion = "Repuesto", Monto = 2500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-4) };
            var envio4 = new Envio { UsuarioId = usuarioId, OrigenDir = "Caballito", DestinoDir = "Lomas", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 1m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Caja chica", Monto = 7000m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-3) };
            var envio5 = new Envio { UsuarioId = usuarioId, OrigenDir = "Morón", DestinoDir = "Lomas", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 5m, Tamano = TamanoEnvio.Grande, Descripcion = "Paquete frágil", Monto = 3500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-2) };

            context.Envios.AddRange(envio1, envio2, envio3, envio4, envio5);
            await context.SaveChangesAsync();

            // --- ASIGNACIONES EFECTIVAS (Vinculan cada viaje con su envío entregado) ---
            context.Asignaciones.AddRange(
                new Asignacion { ViajeId = viaje1.Id, EnvioId = envio1.Id, Ganancia = 4000m, AsignadoEn = DateTime.Now.AddDays(-5), DesvioKm = 1.0m, TiempoExtraMin = 10 },
                new Asignacion { ViajeId = viaje2.Id, EnvioId = envio2.Id, Ganancia = 5500m, AsignadoEn = DateTime.Now.AddDays(-4), DesvioKm = 1.5m, TiempoExtraMin = 12 },
                new Asignacion { ViajeId = viaje3.Id, EnvioId = envio3.Id, Ganancia = 2500m, AsignadoEn = DateTime.Now.AddDays(-3), DesvioKm = 0.8m, TiempoExtraMin = 8 },
                new Asignacion { ViajeId = viaje4.Id, EnvioId = envio4.Id, Ganancia = 7000m, AsignadoEn = DateTime.Now.AddDays(-2), DesvioKm = 2.0m, TiempoExtraMin = 15 },
                new Asignacion { ViajeId = viaje5.Id, EnvioId = envio5.Id, Ganancia = 3500m, AsignadoEn = DateTime.Now.AddDays(-1), DesvioKm = 1.2m, TiempoExtraMin = 10 }
            );

            await context.SaveChangesAsync();
        }
    }
}