namespace RuteaYa.DTOs;

public class HistorialItemDto


{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; 
    public string Descripcion { get; set; } = string.Empty; 
    public string Detalle { get; set; } = string.Empty; 
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
}