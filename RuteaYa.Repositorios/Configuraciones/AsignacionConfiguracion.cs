using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Configuraciones
{
    public class AsignacionConfiguracion : IEntityTypeConfiguration<Asignacion>
    {
        public void Configure(EntityTypeBuilder<Asignacion> builder)
        {
            builder.ToTable("Asignacion");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.DesvioKm).HasPrecision(8, 2);
            builder.Property(a => a.Ganancia).HasPrecision(12, 2);
            builder.Property(a => a.AsignadoEn).HasDefaultValueSql("SYSUTCDATETIME()");

            // Uno a uno: un envío solo puede asignarse una vez (índice único sobre EnvioId).
            builder.HasOne(a => a.Envio)
                .WithOne(e => e.Asignacion)
                .HasForeignKey<Asignacion>(a => a.EnvioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Viaje)
                .WithMany(v => v.Asignaciones)
                .HasForeignKey(a => a.ViajeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.ViajeId, a.FechaViaje });
        }
    }
}
