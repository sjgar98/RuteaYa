namespace RuteaYa.DTOs;

public class PotencialMensualDto
{
	public string Mes { get; set; } = string.Empty;
	public string Criterio { get; set; } = string.Empty;
	public int ViajesAnalizados { get; set; }
	public int PaquetesPosibles { get; set; }
	public decimal GananciaPotencial { get; set; }
	public decimal PromedioPorViaje { get; set; }
	public decimal GananciaPorSemana { get; set; }
	public List<DetalleViajePotencialDto> MejorRecorrido { get; set; } = new();
	public List<DetalleAsignacionPotencialDto> DetalleAsignaciones { get; set; } = new();
}