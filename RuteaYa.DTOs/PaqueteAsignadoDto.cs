using RuteaYa.DTOs.Enums;

namespace RuteaYa.DTOs
{
    // Envío asignado a un viaje, visto desde el conductor.
    public class PaqueteAsignadoDto
    {
        public int EnvioId { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public EstadoEnvio Estado { get; set; }
        public DateOnly FechaViaje { get; set; }
        public DateOnly FechaLimite { get; set; }
        public decimal PesoKg { get; set; }
        public decimal Ganancia { get; set; }
    }
}
