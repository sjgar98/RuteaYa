using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace RuteaYa.Entidades
{
    public class RuteaYaDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Viaje> Viajes { get; set; }
        public DbSet<Parada> Paradas { get; set; }
        public DbSet<ViajeDia> ViajeDias { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<Asignacion> Asignaciones { get; set; }
        public DbSet<EnvioEstadoHistorial> EnvioEstadoHistorial { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ObtenerConexion());
        }

        // Si está definida RUTEAYA_DB_HOST se usa otro servidor (ej: SQL Server en Docker en Linux);
        // si no, LocalDB.
        private static string ObtenerConexion()
        {
            var host = Environment.GetEnvironmentVariable("RUTEAYA_DB_HOST");
            if (string.IsNullOrEmpty(host))
                return @"Server=(localdb)\MSSQLLocalDB;Database=RuteaYa;Trusted_Connection=True;TrustServerCertificate=True";

            var puerto = Environment.GetEnvironmentVariable("RUTEAYA_DB_PORT") ?? "1433";
            var conexion = new SqlConnectionStringBuilder
            {
                DataSource = $"{host},{puerto}",
                InitialCatalog = Environment.GetEnvironmentVariable("RUTEAYA_DB_NAME") ?? "RuteaYa",
                UserID = ObtenerRequerida("RUTEAYA_DB_USER"),
                Password = ObtenerRequerida("RUTEAYA_DB_PASSWORD"),
                TrustServerCertificate = true
            };
            return conexion.ConnectionString;
        }

        private static string ObtenerRequerida(string variable) =>
            Environment.GetEnvironmentVariable(variable)
            ?? throw new InvalidOperationException($"Falta la variable de entorno {variable} (requerida cuando se define RUTEAYA_DB_HOST).");

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Por defecto EF usa decimal(18,2): no alcanza para guardar coordenadas (lat/lng).
            configurationBuilder.Properties<decimal>().HavePrecision(18, 6);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Clave compuesta: un viaje no puede repetir el mismo día.
            modelBuilder.Entity<ViajeDia>().HasKey(d => new { d.ViajeId, d.DiaSemana });

            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();

            // SQL Server no permite dos caminos de borrado en cascada hacia Asignacion
            // (Usuario -> Viaje -> Asignacion y Usuario -> Envio -> Asignacion).
            modelBuilder.Entity<Asignacion>()
                .HasOne(a => a.Viaje)
                .WithMany(v => v.Asignaciones)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
