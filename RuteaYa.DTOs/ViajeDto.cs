using RuteaYa.DTOs.Enums;

namespace RuteaYa.DTOs
{
    public class ViajeDto
    {
        public int Id { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public List<DiaSemana> Dias { get; set; } = new List<DiaSemana>();
        public TimeOnly HoraSalida { get; set; }
        public TimeOnly HoraLlegada { get; set; }
        public int MaxPaquetes { get; set; }
        public decimal PesoMaxKg { get; set; }
        public decimal DesvioMaxKm { get; set; }
        public decimal DistanciaKm { get; set; }
        public int DuracionMin { get; set; }
        public bool Activo { get; set; }

        // Hay envíos asignados sin terminar (asignado, retirado o en camino): el viaje no se puede editar ni desactivar.
        public bool TieneEnviosPendientes { get; set; }
    }
}
