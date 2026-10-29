namespace RuteaYa.DTOs
{
    public class OportunidadDto
    {
        public int EnvioId { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public decimal PesoKg { get; set; }
        public decimal DesvioKm { get; set; }
        public decimal MontoPago { get; set; }
        public string Compatibilidad { get; set; } = string.Empty;
        public decimal PuntuacionCompatibilidad { get; set; }
        public int CapacidadRestantePaquetes { get; set; }
        public decimal CapacidadRestanteKg { get; set; }
    }
}