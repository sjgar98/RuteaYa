using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Configuraciones
{
    public class ParadaConfiguracion : IEntityTypeConfiguration<Parada>
    {
        public void Configure(EntityTypeBuilder<Parada> builder)
        {
            builder.ToTable("Parada");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Direccion).IsRequired().HasMaxLength(300);
            builder.Property(p => p.Lat).HasPrecision(9, 6);
            builder.Property(p => p.Lng).HasPrecision(9, 6);

            builder.HasOne(p => p.Viaje)
                .WithMany(v => v.Paradas)
                .HasForeignKey(p => p.ViajeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Las paradas de un viaje mantienen un orden sin repetidos.
            builder.HasIndex(p => new { p.ViajeId, p.Orden }).IsUnique();
        }
    }
}
