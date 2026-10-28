using RuteaYa.DTOs.Enums;

namespace RuteaYa.DTOs
{
    public class EnvioDetalleDto : EnvioDto
    {
        public decimal OrigenLat { get; set; }
        public decimal OrigenLng { get; set; }
        public decimal DestinoLat { get; set; }
        public decimal DestinoLng { get; set; }

        public ViajeAsociadoDto? ViajeAsociado { get; set; }

        public List<EnvioHistorialDto> Historial { get; set; } = new List<EnvioHistorialDto>();
    }

    public class ViajeAsociadoDto
    {
        public int ViajeId { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public DateOnly FechaViaje { get; set; }
        public decimal DesvioKm { get; set; }
        public decimal Ganancia { get; set; }
    }

    public class EnvioHistorialDto
    {
        public EstadoEnvio? EstadoAnterior { get; set; }
        public EstadoEnvio EstadoNuevo { get; set; }
        public DateTime Fecha { get; set; }
    }
}