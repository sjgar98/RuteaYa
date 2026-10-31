namespace RuteaYa.DTOs
{
    // Envío que el optimizador evaluó pero no puede entrar en ninguna combinación válida.
    public class EnvioDescartadoDto
    {
        public int EnvioId { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PesoKg { get; set; }
        public decimal DesvioKm { get; set; }
        public decimal MontoPago { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}

