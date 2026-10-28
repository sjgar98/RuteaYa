using RuteaYa.DTOs.Enums;

namespace RuteaYa.DTOs
{
    public class EnvioDto
    {
        public int Id { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public DateOnly FechaLimite { get; set; }
        public decimal PesoKg { get; set; }
        public TamanoEnvio Tamano { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public EstadoEnvio Estado { get; set; }
        public bool PuedeCancelar { get; set; }
    }
}