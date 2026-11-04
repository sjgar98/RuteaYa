namespace RuteaYa.DTOs
{
    // Resumen de los envíos elegidos para un viaje antes de reservarlos (Pantalla 11).
    public class ResumenSeleccionDto
    {
        public int ViajeId { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public int MaxPaquetes { get; set; }
        public decimal PesoMaxKg { get; set; }
        public decimal DesvioMaxKm { get; set; }
        public CombinacionDto Seleccion { get; set; } = new CombinacionDto();
        public bool PuedeConfirmar { get; set; }
        public string? Motivo { get; set; }
    }
}
