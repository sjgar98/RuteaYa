using RuteaYa.DTOs.Enums;

namespace RuteaYa.Entidades
{
    public class ViajeDia
    {
        public int ViajeId { get; set; }
        public DiaSemana DiaSemana { get; set; }

        public Viaje Viaje { get; set; } = null!;
    }
}
