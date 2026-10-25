using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Configuraciones
{
    public class ViajeConfiguracion : IEntityTypeConfiguration<Viaje>
    {
        public void Configure(EntityTypeBuilder<Viaje> builder)
        {
            builder.ToTable("Viaje", t =>
            {
                t.HasCheckConstraint("CK_Viaje_MaxPaquetes", "[MaxPaquetes] > 0");
                t.HasCheckConstraint("CK_Viaje_PesoMaxKg", "[PesoMaxKg] > 0");
                t.HasCheckConstraint("CK_Viaje_DesvioMaxKm", "[DesvioMaxKm] >= 0");
            });
            builder.HasKey(v => v.Id);

            builder.Property(v => v.OrigenDir).IsRequired().HasMaxLength(300);
            builder.Property(v => v.OrigenLat).HasPrecision(9, 6);
            builder.Property(v => v.OrigenLng).HasPrecision(9, 6);
            builder.Property(v => v.DestinoDir).IsRequired().HasMaxLength(300);
            builder.Property(v => v.DestinoLat).HasPrecision(9, 6);
            builder.Property(v => v.DestinoLng).HasPrecision(9, 6);

            builder.Property(v => v.PesoMaxKg).HasPrecision(8, 2);
            builder.Property(v => v.DesvioMaxKm).HasPrecision(8, 2);
            builder.Property(v => v.DistanciaKm).HasPrecision(8, 2);

            builder.HasOne(v => v.Usuario)
                .WithMany(u => u.Viajes)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(v => new { v.UsuarioId, v.Activo });
        }
    }
}
