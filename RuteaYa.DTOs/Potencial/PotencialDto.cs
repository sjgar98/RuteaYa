namespace RuteaYa.DTOs
{
    public class PotencialDto
    {
        public string Mes { get; set; } = string.Empty; // Formato "yyyy-MM"
        public string Criterio { get; set; } = "ganancia"; // ganancia, desvio, equilibrio
        public int ViajesAnalizados { get; set; }
        public int PaquetesPosibles { get; set; }
        public decimal GananciaPotencial { get; set; }
        public decimal PromedioPorViaje { get; set; }
        public decimal GananciaPorSemana { get; set; }
        public string MejorRecorrido { get; set; } = string.Empty;
        public List<PotencialDetalleViajeDto> DetalleViajes { get; set; } = new List<PotencialDetalleViajeDto>();
    }
}



