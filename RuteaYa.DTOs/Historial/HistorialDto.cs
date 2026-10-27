namespace RuteaYa.DTOs;

public class HistorialDto
{
    public int ViajesRealizados { get; set; }
    public int EnviosTransportados { get; set; }
    public decimal GananciaEfectiva { get; set; }
    public List<HistorialItemDto> Items { get; set; } = new List<HistorialItemDto>();
}
