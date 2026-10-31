using Microsoft.EntityFrameworkCore;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;

namespace RuteaYa.API.Servicios
{
    public static class InicializadorDatos
    {
        public static async Task InicializarAsync(RuteaYaDbContext context)
        {
            // Si ya existen viajes, evitamos duplicar datos
            if (await context.Viajes.AnyAsync())
            {
                return;
            }

            int usuarioId = 1; // Usuario principal por defecto (prueba@ruteaya.com)

            // ==========================================
            // 1. DATOS PARA LA PANTALLA 13 (HISTORIAL)
            // ==========================================
            var viajeHist1 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Belgrano", DestinoDir = "Lomas de Zamora", HoraSalida = new TimeOnly(8, 0), HoraLlegada = new TimeOnly(8, 55), MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 5m, DistanciaKm = 18.4m, DuracionMin = 55, Activo = false };
            var viajeHist2 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Palermo", DestinoDir = "San Isidro", HoraSalida = new TimeOnly(18, 30), HoraLlegada = new TimeOnly(19, 20), MaxPaquetes = 2, PesoMaxKg = 5m, DesvioMaxKm = 3m, DistanciaKm = 15.0m, DuracionMin = 50, Activo = false };
            var viajeHist3 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Avellaneda", DestinoDir = "Palermo", HoraSalida = new TimeOnly(9, 0), HoraLlegada = new TimeOnly(9, 40), MaxPaquetes = 4, PesoMaxKg = 12m, DesvioMaxKm = 4m, DistanciaKm = 12.0m, DuracionMin = 40, Activo = false };
            var viajeHist4 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Caballito", DestinoDir = "Lomas", HoraSalida = new TimeOnly(14, 0), HoraLlegada = new TimeOnly(14, 45), MaxPaquetes = 3, PesoMaxKg = 8m, DesvioMaxKm = 3m, DistanciaKm = 14.2m, DuracionMin = 45, Activo = false };
            var viajeHist5 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Morón", DestinoDir = "Lomas", HoraSalida = new TimeOnly(10, 0), HoraLlegada = new TimeOnly(11, 0), MaxPaquetes = 5, PesoMaxKg = 15m, DesvioMaxKm = 6m, DistanciaKm = 22.5m, DuracionMin = 60, Activo = false };

            context.Viajes.AddRange(viajeHist1, viajeHist2, viajeHist3, viajeHist4, viajeHist5);
            await context.SaveChangesAsync();

            var envioHist1 = new Envio { UsuarioId = usuarioId, OrigenDir = "Plaza Italia", DestinoDir = "Obelisco", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 2m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Zapatillas", Monto = 4000m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-6) };
            var envioHist2 = new Envio { UsuarioId = usuarioId, OrigenDir = "Recoleta", DestinoDir = "Quilmes", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 4m, Tamano = TamanoEnvio.Mediano, Descripcion = "Documentos", Monto = 5500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-5) };
            var envioHist3 = new Envio { UsuarioId = usuarioId, OrigenDir = "Avellaneda", DestinoDir = "Palermo", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 3m, Tamano = TamanoEnvio.Mediano, Descripcion = "Repuesto", Monto = 2500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-4) };
            var envioHist4 = new Envio { UsuarioId = usuarioId, OrigenDir = "Caballito", DestinoDir = "Lomas", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 1m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Caja chica", Monto = 7000m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-3) };
            var envioHist5 = new Envio { UsuarioId = usuarioId, OrigenDir = "Morón", DestinoDir = "Lomas", FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 5m, Tamano = TamanoEnvio.Grande, Descripcion = "Paquete frágil", Monto = 3500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.Now.AddDays(-2) };

            context.Envios.AddRange(envioHist1, envioHist2, envioHist3, envioHist4, envioHist5);
            await context.SaveChangesAsync();

            context.Asignaciones.AddRange(
                new Asignacion { ViajeId = viajeHist1.Id, EnvioId = envioHist1.Id, Ganancia = 4000m, AsignadoEn = DateTime.Now.AddDays(-5), DesvioKm = 1.0m, TiempoExtraMin = 10 },
                new Asignacion { ViajeId = viajeHist2.Id, EnvioId = envioHist2.Id, Ganancia = 5500m, AsignadoEn = DateTime.Now.AddDays(-4), DesvioKm = 1.5m, TiempoExtraMin = 12 },
                new Asignacion { ViajeId = viajeHist3.Id, EnvioId = envioHist3.Id, Ganancia = 2500m, AsignadoEn = DateTime.Now.AddDays(-3), DesvioKm = 0.8m, TiempoExtraMin = 8 },
                new Asignacion { ViajeId = viajeHist4.Id, EnvioId = envioHist4.Id, Ganancia = 7000m, AsignadoEn = DateTime.Now.AddDays(-2), DesvioKm = 2.0m, TiempoExtraMin = 15 },
                new Asignacion { ViajeId = viajeHist5.Id, EnvioId = envioHist5.Id, Ganancia = 3500m, AsignadoEn = DateTime.Now.AddDays(-1), DesvioKm = 1.2m, TiempoExtraMin = 10 }
            );
            await context.SaveChangesAsync();


            // ==========================================
            // 2. DATOS PARA LA PANTALLA 14 (POTENCIAL MENSUAL)
            // ==========================================

            // Creamos un usuario externo para que posea los envíos disponibles
            var usuarioExterno = new Usuario
            {
                Email = "externo@ruteaya.com"
            };
            context.Usuarios.Add(usuarioExterno);
            await context.SaveChangesAsync(); // Al guardarlo, SQL Server le asigna su ID automáticamente

            // Viajes activos recurrentes del usuario principal
            var viajeRec1 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Centro", DestinoDir = "Nuñez", HoraSalida = new TimeOnly(7, 30), HoraLlegada = new TimeOnly(8, 15), MaxPaquetes = 4, PesoMaxKg = 15m, DesvioMaxKm = 4m, DistanciaKm = 12.0m, DuracionMin = 45, Activo = true };
            var viajeRec2 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Flores", DestinoDir = "Puerto Madero", HoraSalida = new TimeOnly(13, 0), HoraLlegada = new TimeOnly(13, 50), MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 3m, DistanciaKm = 10.5m, DuracionMin = 50, Activo = true };

            context.Viajes.AddRange(viajeRec1, viajeRec2);
            await context.SaveChangesAsync();

            // Días recurrentes para los viajes activos (ej. Lunes, Miércoles, Viernes)
            context.ViajeDias.AddRange(
                new ViajeDia { ViajeId = viajeRec1.Id, DiaSemana = DiaSemana.Lunes },
                new ViajeDia { ViajeId = viajeRec1.Id, DiaSemana = DiaSemana.Miercoles },
                new ViajeDia { ViajeId = viajeRec1.Id, DiaSemana = DiaSemana.Viernes },
                new ViajeDia { ViajeId = viajeRec2.Id, DiaSemana = DiaSemana.Martes },
                new ViajeDia { ViajeId = viajeRec2.Id, DiaSemana = DiaSemana.Jueves }
            );

            // Envíos disponibles asociados al usuario externo creado dinámicamente
            context.Envios.AddRange(
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Obelisco", DestinoDir = "Nuñez", FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(15)), PesoKg = 2.5m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Documentación urgente", Monto = 6500m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.Now },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Retiro", DestinoDir = "Belgrano", FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(15)), PesoKg = 4.0m, Tamano = TamanoEnvio.Mediano, Descripcion = "Ropa", Monto = 8000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.Now },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "San Telmo", DestinoDir = "Palermo", FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(15)), PesoKg = 1.5m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Libros", Monto = 5000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.Now },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Flores", DestinoDir = "Puerto Madero", FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(15)), PesoKg = 5.0m, Tamano = TamanoEnvio.Grande, Descripcion = "Caja Electrónica", Monto = 12000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.Now },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Caballito", DestinoDir = "Microcentro", FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(15)), PesoKg = 3.0m, Tamano = TamanoEnvio.Mediano, Descripcion = "Comida envasada", Monto = 7500m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.Now }
            );

            await context.SaveChangesAsync();
        }
    }
}