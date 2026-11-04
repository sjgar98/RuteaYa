using Microsoft.EntityFrameworkCore;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.Entidades
{
    // Datos de prueba sembrados por migración (HasData) para probar todas las pantallas desde una base vacía.
    // Todos los valores son fijos: HasData compara el seed entre migraciones.
    //  - UsuarioPrueba (conductor): V1 Belgrano → Lomas sin envíos (para el Optimizador: los envíos 1 a 5 de
    //    UsuarioRemitente reproducen el ejemplo de la sección 12 y el 6 queda descartado por desvío),
    //    V2 Palermo → San Isidro lleno con dos envíos en curso y V3 inactivo con entregas.
    //  - UsuarioRemitente: publica los envíos y maneja V4 Recoleta → Quilmes con envíos de UsuarioPrueba.
    public static class DatosIniciales
    {
        public static class UsuarioRemitente
        {
            public const int Id = 2;
            public const string Nombre = "María";
            public const string Apellido = "Gómez";
            public const string Email = "remitente@ruteaya.com";
            public const string Password = UsuarioPrueba.Password;
            public const string PasswordHash = UsuarioPrueba.PasswordHash;
        }

        public static readonly DateOnly FechaLimiteDisponibles = new DateOnly(2027, 6, 30);

        private record Lugar(string Direccion, decimal Lat, decimal Lng);

        private static readonly Lugar Belgrano = new("Belgrano, CABA", -34.562700m, -58.456500m);
        private static readonly Lugar Palermo = new("Palermo, CABA", -34.588900m, -58.430600m);
        private static readonly Lugar PlazaItalia = new("Plaza Italia, Palermo, CABA", -34.581000m, -58.420800m);
        private static readonly Lugar Recoleta = new("Recoleta, CABA", -34.587500m, -58.397400m);
        private static readonly Lugar Caballito = new("Caballito, CABA", -34.618900m, -58.442200m);
        private static readonly Lugar Barracas = new("Barracas, CABA", -34.645300m, -58.382600m);
        private static readonly Lugar Avellaneda = new("Avellaneda, Buenos Aires", -34.662700m, -58.365300m);
        private static readonly Lugar Lomas = new("Lomas de Zamora, Buenos Aires", -34.760900m, -58.406300m);
        private static readonly Lugar Quilmes = new("Quilmes, Buenos Aires", -34.720600m, -58.254600m);
        private static readonly Lugar Moron = new("Morón, Buenos Aires", -34.653400m, -58.619800m);
        private static readonly Lugar SanIsidro = new("San Isidro, Buenos Aires", -34.470800m, -58.528600m);

        public static void Sembrar(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = UsuarioPrueba.Id,
                    Nombre = UsuarioPrueba.Nombre,
                    Apellido = UsuarioPrueba.Apellido,
                    Email = UsuarioPrueba.Email,
                    PasswordHash = UsuarioPrueba.PasswordHash,
                    CreadoEn = new DateTime(2026, 1, 1)
                },
                new Usuario
                {
                    Id = UsuarioRemitente.Id,
                    Nombre = UsuarioRemitente.Nombre,
                    Apellido = UsuarioRemitente.Apellido,
                    Email = UsuarioRemitente.Email,
                    PasswordHash = UsuarioRemitente.PasswordHash,
                    CreadoEn = new DateTime(2026, 1, 1)
                });

            modelBuilder.Entity<Viaje>().HasData(
                CrearViaje(1, UsuarioPrueba.Id, Belgrano, Lomas, new TimeOnly(8, 0), new TimeOnly(8, 55), 3, 10m, 5m, 18.4m, 55, activo: true),
                CrearViaje(2, UsuarioPrueba.Id, Palermo, SanIsidro, new TimeOnly(18, 30), new TimeOnly(19, 20), 2, 5m, 3m, 21.5m, 50, activo: true),
                CrearViaje(3, UsuarioPrueba.Id, Avellaneda, Palermo, new TimeOnly(9, 0), new TimeOnly(9, 40), 4, 12m, 4m, 12.0m, 40, activo: false),
                CrearViaje(4, UsuarioRemitente.Id, Recoleta, Quilmes, new TimeOnly(10, 0), new TimeOnly(10, 50), 3, 8m, 4m, 21.0m, 50, activo: true));

            modelBuilder.Entity<ViajeDia>().HasData(
                new ViajeDia { ViajeId = 1, DiaSemana = DiaSemana.Lunes },
                new ViajeDia { ViajeId = 1, DiaSemana = DiaSemana.Miercoles },
                new ViajeDia { ViajeId = 1, DiaSemana = DiaSemana.Viernes },
                new ViajeDia { ViajeId = 2, DiaSemana = DiaSemana.Viernes },
                new ViajeDia { ViajeId = 3, DiaSemana = DiaSemana.Martes },
                new ViajeDia { ViajeId = 3, DiaSemana = DiaSemana.Jueves },
                new ViajeDia { ViajeId = 4, DiaSemana = DiaSemana.Sabado });

            var envios = new List<Envio>
            {
                CrearEnvio(1, UsuarioRemitente.Id, Caballito, Lomas, "Caja con zapatillas", 2m, TamanoEnvio.Pequeno, 4000m, EstadoEnvio.BuscandoConductor, new DateTime(2026, 10, 28, 13, 0, 0)),
                CrearEnvio(2, UsuarioRemitente.Id, Palermo, Lomas, "Documentos y carpetas", 6m, TamanoEnvio.Mediano, 5500m, EstadoEnvio.BuscandoConductor, new DateTime(2026, 10, 28, 13, 10, 0)),
                CrearEnvio(3, UsuarioRemitente.Id, PlazaItalia, Caballito, "Caja de herramientas", 4m, TamanoEnvio.Mediano, 3500m, EstadoEnvio.BuscandoConductor, new DateTime(2026, 10, 29, 14, 0, 0)),
                CrearEnvio(4, UsuarioRemitente.Id, Palermo, Avellaneda, "Monitor", 8m, TamanoEnvio.Grande, 7000m, EstadoEnvio.BuscandoConductor, new DateTime(2026, 10, 29, 14, 20, 0)),
                CrearEnvio(5, UsuarioRemitente.Id, Barracas, Lomas, "Libro", 1m, TamanoEnvio.Pequeno, 2500m, EstadoEnvio.BuscandoConductor, new DateTime(2026, 10, 30, 12, 0, 0)),
                CrearEnvio(6, UsuarioRemitente.Id, PlazaItalia, SanIsidro, "Zapatillas", 1.5m, TamanoEnvio.Pequeno, 4000m, EstadoEnvio.BuscandoConductor, new DateTime(2026, 10, 30, 12, 30, 0)),
                CrearEnvio(7, UsuarioRemitente.Id, Belgrano, SanIsidro, "Bolso de mano", 2m, TamanoEnvio.Pequeno, 3000m, EstadoEnvio.Retirado, new DateTime(2026, 10, 31, 15, 0, 0)),
                CrearEnvio(8, UsuarioRemitente.Id, PlazaItalia, SanIsidro, "Caja de vinos", 3m, TamanoEnvio.Mediano, 4500m, EstadoEnvio.EnCamino, new DateTime(2026, 10, 26, 15, 0, 0)),
                CrearEnvio(9, UsuarioRemitente.Id, Avellaneda, Palermo, "Repuestos de auto", 3m, TamanoEnvio.Mediano, 3000m, EstadoEnvio.Entregado, new DateTime(2026, 9, 3, 15, 0, 0)),
                CrearEnvio(10, UsuarioRemitente.Id, Barracas, Palermo, "Notebook", 2m, TamanoEnvio.Pequeno, 5000m, EstadoEnvio.Entregado, new DateTime(2026, 9, 18, 15, 0, 0)),
                CrearEnvio(11, UsuarioRemitente.Id, Avellaneda, Recoleta, "Cuadro", 4m, TamanoEnvio.Grande, 4500m, EstadoEnvio.Entregado, new DateTime(2026, 9, 30, 15, 0, 0)),
                CrearEnvio(12, UsuarioRemitente.Id, Caballito, Moron, "Ropa", 2m, TamanoEnvio.Pequeno, 2000m, EstadoEnvio.Cancelado, new DateTime(2026, 10, 10, 15, 0, 0)),
                CrearEnvio(13, UsuarioPrueba.Id, Avellaneda, Palermo, "Repuesto", 1m, TamanoEnvio.Pequeno, 2500m, EstadoEnvio.Entregado, new DateTime(2026, 10, 18, 15, 0, 0)),
                CrearEnvio(14, UsuarioPrueba.Id, Recoleta, Quilmes, "Documentos", 0.5m, TamanoEnvio.Pequeno, 3500m, EstadoEnvio.ConductorAsignado, new DateTime(2026, 10, 30, 15, 0, 0)),
                CrearEnvio(15, UsuarioPrueba.Id, SanIsidro, Belgrano, "Caja de libros", 5m, TamanoEnvio.Mediano, 3000m, EstadoEnvio.BuscandoConductor, new DateTime(2026, 11, 1, 15, 0, 0)),
                CrearEnvio(16, UsuarioPrueba.Id, PlazaItalia, Caballito, "Regalo", 1m, TamanoEnvio.Pequeno, 2000m, EstadoEnvio.Cancelado, new DateTime(2026, 9, 12, 15, 0, 0))
            };
            modelBuilder.Entity<Envio>().HasData(envios);

            var asignaciones = new List<Asignacion>
            {
                CrearAsignacion(1, envioId: 7, viajeId: 2, new DateOnly(2026, 11, 6), 1.2m, 6, 3000m, new DateTime(2026, 11, 2, 14, 0, 0)),
                CrearAsignacion(2, envioId: 8, viajeId: 2, new DateOnly(2026, 10, 30), 0.8m, 4, 4500m, new DateTime(2026, 10, 28, 14, 0, 0)),
                CrearAsignacion(3, envioId: 9, viajeId: 3, new DateOnly(2026, 9, 8), 1.0m, 5, 3000m, new DateTime(2026, 9, 5, 14, 0, 0)),
                CrearAsignacion(4, envioId: 10, viajeId: 3, new DateOnly(2026, 9, 24), 1.5m, 7, 5000m, new DateTime(2026, 9, 20, 14, 0, 0)),
                CrearAsignacion(5, envioId: 11, viajeId: 3, new DateOnly(2026, 10, 6), 2.0m, 9, 4500m, new DateTime(2026, 10, 2, 14, 0, 0)),
                CrearAsignacion(6, envioId: 13, viajeId: 4, new DateOnly(2026, 10, 24), 1.1m, 5, 2500m, new DateTime(2026, 10, 20, 14, 0, 0)),
                CrearAsignacion(7, envioId: 14, viajeId: 4, new DateOnly(2026, 11, 7), 0.7m, 3, 3500m, new DateTime(2026, 11, 1, 14, 0, 0))
            };
            modelBuilder.Entity<Asignacion>().HasData(asignaciones);

            modelBuilder.Entity<EnvioEstadoHistorial>().HasData(CrearHistorial(envios, asignaciones));
        }

        private static Viaje CrearViaje(int id, int usuarioId, Lugar origen, Lugar destino, TimeOnly salida, TimeOnly llegada,
            int maxPaquetes, decimal pesoMaxKg, decimal desvioMaxKm, decimal distanciaKm, int duracionMin, bool activo)
        {
            return new Viaje
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = origen.Direccion,
                OrigenLat = origen.Lat,
                OrigenLng = origen.Lng,
                DestinoDir = destino.Direccion,
                DestinoLat = destino.Lat,
                DestinoLng = destino.Lng,
                HoraSalida = salida,
                HoraLlegada = llegada,
                MaxPaquetes = maxPaquetes,
                PesoMaxKg = pesoMaxKg,
                DesvioMaxKm = desvioMaxKm,
                DistanciaKm = distanciaKm,
                DuracionMin = duracionMin,
                Activo = activo
            };
        }

        private static Envio CrearEnvio(int id, int usuarioId, Lugar origen, Lugar destino, string descripcion, decimal pesoKg,
            TamanoEnvio tamano, decimal monto, EstadoEnvio estado, DateTime creadoEnUtc)
        {
            return new Envio
            {
                Id = id,
                UsuarioId = usuarioId,
                OrigenDir = origen.Direccion,
                OrigenLat = origen.Lat,
                OrigenLng = origen.Lng,
                DestinoDir = destino.Direccion,
                DestinoLat = destino.Lat,
                DestinoLng = destino.Lng,
                FechaLimite = estado == EstadoEnvio.BuscandoConductor ? FechaLimiteDisponibles : DateOnly.FromDateTime(creadoEnUtc).AddDays(30),
                PesoKg = pesoKg,
                Tamano = tamano,
                Descripcion = descripcion,
                Monto = monto,
                Estado = estado,
                CreadoEn = creadoEnUtc
            };
        }

        private static Asignacion CrearAsignacion(int id, int envioId, int viajeId, DateOnly fechaViaje, decimal desvioKm,
            int tiempoExtraMin, decimal ganancia, DateTime asignadoEnUtc)
        {
            return new Asignacion
            {
                Id = id,
                EnvioId = envioId,
                ViajeId = viajeId,
                FechaViaje = fechaViaje,
                DesvioKm = desvioKm,
                TiempoExtraMin = tiempoExtraMin,
                Ganancia = ganancia,
                AsignadoEn = asignadoEnUtc
            };
        }

        // Cada envío recorre las transiciones de EnvioEstadoMaquina hasta su estado actual: publicado, asignado
        // al aceptarlo, retirado y en camino el día del viaje y entregado una hora después; los cancelados, dos días después de publicarse.
        private static List<EnvioEstadoHistorial> CrearHistorial(List<Envio> envios, List<Asignacion> asignaciones)
        {
            var historial = new List<EnvioEstadoHistorial>();

            void Agregar(int envioId, EstadoEnvio? anterior, EstadoEnvio nuevo, DateTime fechaUtc)
            {
                historial.Add(new EnvioEstadoHistorial
                {
                    Id = historial.Count + 1,
                    EnvioId = envioId,
                    EstadoAnterior = anterior,
                    EstadoNuevo = nuevo,
                    Fecha = fechaUtc
                });
            }

            var recorrido = new[] { EstadoEnvio.ConductorAsignado, EstadoEnvio.Retirado, EstadoEnvio.EnCamino, EstadoEnvio.Entregado };

            foreach (var envio in envios)
            {
                Agregar(envio.Id, null, EstadoEnvio.BuscandoConductor, envio.CreadoEn);

                if (envio.Estado == EstadoEnvio.Cancelado)
                {
                    Agregar(envio.Id, EstadoEnvio.BuscandoConductor, EstadoEnvio.Cancelado, envio.CreadoEn.AddDays(2));
                    continue;
                }

                var asignacion = asignaciones.SingleOrDefault(a => a.EnvioId == envio.Id);
                if (asignacion == null)
                {
                    continue;
                }

                var salidaUtc = asignacion.FechaViaje.ToDateTime(new TimeOnly(12, 0));
                var fechas = new[] { asignacion.AsignadoEn, salidaUtc, salidaUtc.AddMinutes(10), salidaUtc.AddHours(1) };
                var anterior = EstadoEnvio.BuscandoConductor;

                for (var i = 0; i < recorrido.Length && anterior != envio.Estado; i++)
                {
                    Agregar(envio.Id, anterior, recorrido[i], fechas[i]);
                    anterior = recorrido[i];
                }
            }

            return historial;
        }
    }
}
