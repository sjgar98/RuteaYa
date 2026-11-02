namespace RuteaYa.DTOs
{
    // Una salida concreta de un viaje recurrente (el viaje en una fecha).
    public class ProximaSalidaDto
    {
        public int ViajeId { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraSalida { get; set; }
        public TimeOnly HoraLlegada { get; set; }
    }
}
