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

            // Asegurar que el usuario principal exista si la BD arranca limpia
            if (!await context.Usuarios.AnyAsync(u => u.Id == usuarioId))
            {
                context.Usuarios.Add(new Usuario
                {
                    Id = usuarioId,
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Email = "prueba@ruteaya.com",
                    PasswordHash = "AQAAAAIAAYagAAAAEP...",
                    CreadoEn = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 1. DATOS PARA LA PANTALLA 13 (HISTORIAL)
            // ==========================================
            var viajeHist1 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Belgrano", OrigenLat = -34.5627m, OrigenLng = -58.4556m, DestinoDir = "Lomas de Zamora", DestinoLat = -34.7578m, DestinoLng = -58.4013m, HoraSalida = new TimeOnly(8, 0), HoraLlegada = new TimeOnly(8, 55), MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 5m, DistanciaKm = 18.4m, DuracionMin = 55, Activo = false };
            var viajeHist2 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Palermo", OrigenLat = -34.5885m, OrigenLng = -58.4308m, DestinoDir = "San Isidro", DestinoLat = -34.4717m, DestinoLng = -58.5147m, HoraSalida = new TimeOnly(18, 30), HoraLlegada = new TimeOnly(19, 20), MaxPaquetes = 2, PesoMaxKg = 5m, DesvioMaxKm = 3m, DistanciaKm = 15.0m, DuracionMin = 50, Activo = false };
            var viajeHist3 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Avellaneda", OrigenLat = -34.6625m, OrigenLng = -58.3647m, DestinoDir = "Palermo", DestinoLat = -34.5885m, DestinoLng = -58.4308m, HoraSalida = new TimeOnly(9, 0), HoraLlegada = new TimeOnly(9, 40), MaxPaquetes = 4, PesoMaxKg = 12m, DesvioMaxKm = 4m, DistanciaKm = 12.0m, DuracionMin = 40, Activo = false };
            var viajeHist4 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Caballito", OrigenLat = -34.6175m, OrigenLng = -58.4419m, DestinoDir = "Lomas", DestinoLat = -34.7578m, DestinoLng = -58.4013m, HoraSalida = new TimeOnly(14, 0), HoraLlegada = new TimeOnly(14, 45), MaxPaquetes = 3, PesoMaxKg = 8m, DesvioMaxKm = 3m, DistanciaKm = 14.2m, DuracionMin = 45, Activo = false };
            var viajeHist5 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Morón", OrigenLat = -34.6534m, OrigenLng = -58.6198m, DestinoDir = "Lomas", DestinoLat = -34.7578m, DestinoLng = -58.4013m, HoraSalida = new TimeOnly(10, 0), HoraLlegada = new TimeOnly(11, 0), MaxPaquetes = 5, PesoMaxKg = 15m, DesvioMaxKm = 6m, DistanciaKm = 22.5m, DuracionMin = 60, Activo = false };

            context.Viajes.AddRange(viajeHist1, viajeHist2, viajeHist3, viajeHist4, viajeHist5);
            await context.SaveChangesAsync();

            context.ViajeDias.AddRange(
                new ViajeDia { ViajeId = viajeHist1.Id, DiaSemana = DiaSemana.Lunes },
                new ViajeDia { ViajeId = viajeHist1.Id, DiaSemana = DiaSemana.Miercoles },
                new ViajeDia { ViajeId = viajeHist1.Id, DiaSemana = DiaSemana.Viernes },
                new ViajeDia { ViajeId = viajeHist2.Id, DiaSemana = DiaSemana.Viernes },
                new ViajeDia { ViajeId = viajeHist3.Id, DiaSemana = DiaSemana.Martes },
                new ViajeDia { ViajeId = viajeHist4.Id, DiaSemana = DiaSemana.Jueves },
                new ViajeDia { ViajeId = viajeHist5.Id, DiaSemana = DiaSemana.Sabado }
            );
            await context.SaveChangesAsync();

            var envioHist1 = new Envio { UsuarioId = usuarioId, OrigenDir = "Plaza Italia", OrigenLat = -34.5831m, OrigenLng = -58.4168m, DestinoDir = "Obelisco", DestinoLat = -34.6037m, DestinoLng = -58.3816m, FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 2m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Zapatillas", Monto = 4000m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.UtcNow.AddDays(-6) };
            var envioHist2 = new Envio { UsuarioId = usuarioId, OrigenDir = "Recoleta", OrigenLat = -34.5896m, OrigenLng = -58.3974m, DestinoDir = "Quilmes", DestinoLat = -34.7225m, DestinoLng = -58.2547m, FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 4m, Tamano = TamanoEnvio.Mediano, Descripcion = "Documentos", Monto = 5500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.UtcNow.AddDays(-5) };
            var envioHist3 = new Envio { UsuarioId = usuarioId, OrigenDir = "Avellaneda", OrigenLat = -34.6625m, OrigenLng = -58.3647m, DestinoDir = "Palermo", DestinoLat = -34.5885m, DestinoLng = -58.4308m, FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 3m, Tamano = TamanoEnvio.Mediano, Descripcion = "Repuesto", Monto = 2500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.UtcNow.AddDays(-4) };
            var envioHist4 = new Envio { UsuarioId = usuarioId, OrigenDir = "Caballito", OrigenLat = -34.6175m, OrigenLng = -58.4419m, DestinoDir = "Lomas", DestinoLat = -34.7578m, DestinoLng = -58.4013m, FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 1m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Caja chica", Monto = 7000m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.UtcNow.AddDays(-3) };
            var envioHist5 = new Envio { UsuarioId = usuarioId, OrigenDir = "Morón", OrigenLat = -34.6534m, OrigenLng = -58.6198m, DestinoDir = "Lomas", DestinoLat = -34.7578m, DestinoLng = -58.4013m, FechaLimite = DateOnly.FromDateTime(DateTime.Today), PesoKg = 5m, Tamano = TamanoEnvio.Grande, Descripcion = "Paquete frágil", Monto = 3500m, Estado = EstadoEnvio.Entregado, CreadoEn = DateTime.UtcNow.AddDays(-2) };

            context.Envios.AddRange(envioHist1, envioHist2, envioHist3, envioHist4, envioHist5);
            await context.SaveChangesAsync();

            context.Asignaciones.AddRange(
                new Asignacion { ViajeId = viajeHist1.Id, EnvioId = envioHist1.Id, Ganancia = 4000m, AsignadoEn = DateTime.UtcNow.AddDays(-5), DesvioKm = 1.0m, TiempoExtraMin = 10 },
                new Asignacion { ViajeId = viajeHist2.Id, EnvioId = envioHist2.Id, Ganancia = 5500m, AsignadoEn = DateTime.UtcNow.AddDays(-4), DesvioKm = 1.5m, TiempoExtraMin = 12 },
                new Asignacion { ViajeId = viajeHist3.Id, EnvioId = envioHist3.Id, Ganancia = 2500m, AsignadoEn = DateTime.UtcNow.AddDays(-3), DesvioKm = 0.8m, TiempoExtraMin = 8 },
                new Asignacion { ViajeId = viajeHist4.Id, EnvioId = envioHist4.Id, Ganancia = 7000m, AsignadoEn = DateTime.UtcNow.AddDays(-2), DesvioKm = 2.0m, TiempoExtraMin = 15 },
                new Asignacion { ViajeId = viajeHist5.Id, EnvioId = envioHist5.Id, Ganancia = 3500m, AsignadoEn = DateTime.UtcNow.AddDays(-1), DesvioKm = 1.2m, TiempoExtraMin = 10 }
            );
            await context.SaveChangesAsync();


            // ==========================================
            // 2. DATOS PARA LA PANTALLA 14 (POTENCIAL MENSUAL) - EXACTAMENTE 8 VIAJES ACTIVOS
            // ==========================================

            var usuarioExterno = new Usuario
            {
                Nombre = "Externo",
                Apellido = "Test",
                Email = "externo@ruteaya.com",
                PasswordHash = "AQAAAAIAAYagAAAAEP...",
                CreadoEn = DateTime.UtcNow
            };
            context.Usuarios.Add(usuarioExterno);
            await context.SaveChangesAsync();

            // Creamos 8 viajes activos recurrentes bien distribuidos
            var v1 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Centro", OrigenLat = -34.6037m, OrigenLng = -58.3816m, DestinoDir = "Nuñez", DestinoLat = -34.5453m, DestinoLng = -58.4638m, HoraSalida = new TimeOnly(7, 30), HoraLlegada = new TimeOnly(8, 15), MaxPaquetes = 4, PesoMaxKg = 15m, DesvioMaxKm = 4m, DistanciaKm = 12.0m, DuracionMin = 45, Activo = true };
            var v2 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Flores", OrigenLat = -34.6300m, OrigenLng = -58.4600m, DestinoDir = "Puerto Madero", DestinoLat = -34.6100m, DestinoLng = -58.3600m, HoraSalida = new TimeOnly(13, 0), HoraLlegada = new TimeOnly(13, 50), MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 3m, DistanciaKm = 10.5m, DuracionMin = 50, Activo = true };
            var v3 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Belgrano", OrigenLat = -34.5627m, OrigenLng = -58.4556m, DestinoDir = "Quilmes", DestinoLat = -34.7225m, DestinoLng = -58.2547m, HoraSalida = new TimeOnly(17, 0), HoraLlegada = new TimeOnly(18, 10), MaxPaquetes = 5, PesoMaxKg = 20m, DesvioMaxKm = 6m, DistanciaKm = 25.0m, DuracionMin = 70, Activo = true };
            var v4 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Caballito", OrigenLat = -34.6175m, OrigenLng = -58.4419m, DestinoDir = "San Isidro", DestinoLat = -34.4717m, DestinoLng = -58.5147m, HoraSalida = new TimeOnly(8, 30), HoraLlegada = new TimeOnly(9, 20), MaxPaquetes = 4, PesoMaxKg = 12m, DesvioMaxKm = 4m, DistanciaKm = 19.0m, DuracionMin = 50, Activo = true };
            var v5 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Palermo", OrigenLat = -34.5885m, OrigenLng = -58.4308m, DestinoDir = "Avellaneda", DestinoLat = -34.6625m, DestinoLng = -58.3647m, HoraSalida = new TimeOnly(7, 0), HoraLlegada = new TimeOnly(7, 45), MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 3m, DistanciaKm = 11.2m, DuracionMin = 45, Activo = true };
            var v6 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Liniers", OrigenLat = -34.6437m, OrigenLng = -58.5348m, DestinoDir = "Vicente López", DestinoLat = -34.5281m, DestinoLng = -58.4756m, HoraSalida = new TimeOnly(8, 0), HoraLlegada = new TimeOnly(9, 0), MaxPaquetes = 4, PesoMaxKg = 15m, DesvioMaxKm = 5m, DistanciaKm = 20.1m, DuracionMin = 60, Activo = true };
            var v7 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Almagro", OrigenLat = -34.6111m, OrigenLng = -58.4200m, DestinoDir = "Tigre", DestinoLat = -34.4258m, DestinoLng = -58.5796m, HoraSalida = new TimeOnly(6, 30), HoraLlegada = new TimeOnly(7, 40), MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 4m, DistanciaKm = 28.5m, DuracionMin = 70, Activo = true };
            var v8 = new Viaje { UsuarioId = usuarioId, OrigenDir = "Recoleta", OrigenLat = -34.5896m, OrigenLng = -58.3974m, DestinoDir = "Lanús", DestinoLat = -34.7094m, DestinoLng = -58.3869m, HoraSalida = new TimeOnly(12, 0), HoraLlegada = new TimeOnly(12, 50), MaxPaquetes = 4, PesoMaxKg = 12m, DesvioMaxKm = 4m, DistanciaKm = 14.3m, DuracionMin = 50, Activo = true };

            var listaViajesActivos = new List<Viaje> { v1, v2, v3, v4, v5, v6, v7, v8 };
            context.Viajes.AddRange(listaViajesActivos);
            await context.SaveChangesAsync();

            // Asignar días de la semana para los 8 viajes
            var listaViajeDias = new List<ViajeDia>();
            foreach (var viaje in listaViajesActivos)
            {
                listaViajeDias.Add(new ViajeDia { ViajeId = viaje.Id, DiaSemana = DiaSemana.Lunes });
                listaViajeDias.Add(new ViajeDia { ViajeId = viaje.Id, DiaSemana = DiaSemana.Miercoles });
                listaViajeDias.Add(new ViajeDia { ViajeId = viaje.Id, DiaSemana = DiaSemana.Viernes });
            }

            context.ViajeDias.AddRange(listaViajeDias);
            await context.SaveChangesAsync();

            // Envíos disponibles creados por el usuario externo (10 envíos óptimos)
            context.Envios.AddRange(
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Obelisco", OrigenLat = -34.6037m, OrigenLng = -58.3816m, DestinoDir = "Nuñez", DestinoLat = -34.5453m, DestinoLng = -58.4638m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 2.5m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Documentación urgente", Monto = 6500m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Retiro", OrigenLat = -34.5926m, OrigenLng = -58.3747m, DestinoDir = "Belgrano", DestinoLat = -34.5627m, DestinoLng = -58.4556m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 4.0m, Tamano = TamanoEnvio.Mediano, Descripcion = "Ropa y accesorios", Monto = 8000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "San Telmo", OrigenLat = -34.6214m, OrigenLng = -58.3731m, DestinoDir = "Palermo", DestinoLat = -34.5885m, DestinoLng = -58.4308m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 1.5m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Libros técnicos", Monto = 5000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Flores", OrigenLat = -34.6300m, OrigenLng = -58.4600m, DestinoDir = "Puerto Madero", DestinoLat = -34.6100m, DestinoLng = -58.3600m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 5.0m, Tamano = TamanoEnvio.Grande, Descripcion = "Caja Electrónica", Monto = 12000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Caballito", OrigenLat = -34.6175m, OrigenLng = -58.4419m, DestinoDir = "Microcentro", DestinoLat = -34.6037m, DestinoLng = -58.3816m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 3.0m, Tamano = TamanoEnvio.Mediano, Descripcion = "Comida envasada", Monto = 7500m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Constitución", OrigenLat = -34.6283m, OrigenLng = -58.3811m, DestinoDir = "Quilmes", DestinoLat = -34.7225m, DestinoLng = -58.2547m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 6.0m, Tamano = TamanoEnvio.Grande, Descripcion = "Repuestos mecánicos", Monto = 14000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Palermo", OrigenLat = -34.5885m, OrigenLng = -58.4308m, DestinoDir = "San Isidro", DestinoLat = -34.4717m, DestinoLng = -58.5147m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 2.0m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Cosméticos", Monto = 6000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Belgrano", OrigenLat = -34.5627m, OrigenLng = -58.4556m, DestinoDir = "Nuñez", DestinoLat = -34.5453m, DestinoLng = -58.4638m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 1.0m, Tamano = TamanoEnvio.Pequeno, Descripcion = "Sobres de papel", Monto = 3500m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Centro", OrigenLat = -34.6037m, OrigenLng = -58.3816m, DestinoDir = "Puerto Madero", DestinoLat = -34.6100m, DestinoLng = -58.3600m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 3.5m, Tamano = TamanoEnvio.Mediano, Descripcion = "Hardware PC", Monto = 9500m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow },
                new Envio { UsuarioId = usuarioExterno.Id, OrigenDir = "Liniers", OrigenLat = -34.6437m, OrigenLng = -58.5348m, DestinoDir = "Vicente López", DestinoLat = -34.5281m, DestinoLng = -58.4756m, FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(40)), PesoKg = 3.0m, Tamano = TamanoEnvio.Mediano, Descripcion = "Repuestos de auto", Monto = 9000m, Estado = EstadoEnvio.BuscandoConductor, CreadoEn = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();
        }
    }
}