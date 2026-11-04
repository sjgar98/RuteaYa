namespace RuteaYa.DTOs;

public class DetalleAsignacionPotencialDto
{
    public int EnvioId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string OrigenDestinoEnvio { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateOnly FechaViaje { get; set; }
    public string ViajeAsociado { get; set; } = string.Empty;
}