using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;

namespace RuteaYa.Repositorios
{
    public class RuteaYaDbContext : DbContext
    {
        public RuteaYaDbContext(DbContextOptions<RuteaYaDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Viaje> Viajes => Set<Viaje>();
        public DbSet<Parada> Paradas => Set<Parada>();
        public DbSet<ViajeDia> ViajeDias => Set<ViajeDia>();
        public DbSet<Envio> Envios => Set<Envio>();
        public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
        public DbSet<EnvioEstadoHistorial> EnvioEstadoHistorial => Set<EnvioEstadoHistorial>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RuteaYaDbContext).Assembly);
        }
    }
}
