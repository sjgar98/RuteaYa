namespace RuteaYa.DTOs;

public class DetalleViajePotencialDto
{
    public int ViajeId { get; set; }
    public string OrigenDestino { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public int SalidasEnPeriodo { get; set; }
    public decimal GananciaEstimada { get; set; }
    public int PaquetesAsignados { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal DesvioTotalKm { get; set; }
}