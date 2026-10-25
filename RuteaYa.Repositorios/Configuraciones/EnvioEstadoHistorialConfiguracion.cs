using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Configuraciones
{
    public class EnvioEstadoHistorialConfiguracion : IEntityTypeConfiguration<EnvioEstadoHistorial>
    {
        public void Configure(EntityTypeBuilder<EnvioEstadoHistorial> builder)
        {
            builder.ToTable("EnvioEstadoHistorial");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Fecha).HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasOne(h => h.Envio)
                .WithMany(e => e.Historial)
                .HasForeignKey(h => h.EnvioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
