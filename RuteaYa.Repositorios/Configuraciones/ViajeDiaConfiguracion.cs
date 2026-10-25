using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Configuraciones
{
    public class ViajeDiaConfiguracion : IEntityTypeConfiguration<ViajeDia>
    {
        public void Configure(EntityTypeBuilder<ViajeDia> builder)
        {
            builder.ToTable("ViajeDia", t =>
                t.HasCheckConstraint("CK_ViajeDia_DiaSemana", "[DiaSemana] BETWEEN 1 AND 7"));
            builder.HasKey(d => new { d.ViajeId, d.DiaSemana });

            builder.HasOne(d => d.Viaje)
                .WithMany(v => v.Dias)
                .HasForeignKey(d => d.ViajeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
