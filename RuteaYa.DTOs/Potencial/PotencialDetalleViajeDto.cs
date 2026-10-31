namespace RuteaYa.DTOs
{

	public class PotencialDetalleViajeDto
	{
		public int ViajeId { get; set; }
		public string ViajeDescripcion { get; set; } = string.Empty;
		public DateTime FechaOcurrencia { get; set; }
		public int CantidadPaquetes { get; set; }
		public decimal PesoTotalKg { get; set; }
		public decimal DesvioTotalKm { get; set; }
		public decimal GananciaEstimada { get; set; }
		public List<string> PaquetesAsignados { get; set; } = new List<string>();
	}
}