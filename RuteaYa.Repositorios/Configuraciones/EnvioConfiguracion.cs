using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Configuraciones
{
    public class EnvioConfiguracion : IEntityTypeConfiguration<Envio>
    {
        public void Configure(EntityTypeBuilder<Envio> builder)
        {
            builder.ToTable("Envio", t =>
            {
                t.HasCheckConstraint("CK_Envio_PesoKg", "[PesoKg] > 0");
                t.HasCheckConstraint("CK_Envio_Monto", "[Monto] > 0");
            });
            builder.HasKey(e => e.Id);

            builder.Property(e => e.OrigenDir).IsRequired().HasMaxLength(300);
            builder.Property(e => e.OrigenLat).HasPrecision(9, 6);
            builder.Property(e => e.OrigenLng).HasPrecision(9, 6);
            builder.Property(e => e.DestinoDir).IsRequired().HasMaxLength(300);
            builder.Property(e => e.DestinoLat).HasPrecision(9, 6);
            builder.Property(e => e.DestinoLng).HasPrecision(9, 6);

            builder.Property(e => e.PesoKg).HasPrecision(8, 2);
            builder.Property(e => e.Monto).HasPrecision(12, 2);
            builder.Property(e => e.Descripcion).IsRequired().HasMaxLength(500);
            builder.Property(e => e.CreadoEn).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.Property(e => e.RowVersion).IsRowVersion();

            builder.HasOne(e => e.Usuario)
                .WithMany(u => u.Envios)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => new { e.Estado, e.FechaLimite });
            builder.HasIndex(e => e.UsuarioId);
        }
    }
}
