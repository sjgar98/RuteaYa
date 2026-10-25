using RuteaYa.Entidades.Enums;

namespace RuteaYa.Entidades
{
    public class Envio
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        public string OrigenDir { get; set; } = string.Empty;
        public decimal OrigenLat { get; set; }
        public decimal OrigenLng { get; set; }

        public string DestinoDir { get; set; } = string.Empty;
        public decimal DestinoLat { get; set; }
        public decimal DestinoLng { get; set; }

        public DateOnly FechaLimite { get; set; }
        public decimal PesoKg { get; set; }
        public TamanoEnvio Tamano { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public EstadoEnvio Estado { get; set; } = EstadoEnvio.BuscandoConductor;
        public DateTime CreadoEn { get; set; }

        // Token de concurrencia: evita que dos conductores tomen el mismo envío a la vez.
        public byte[] RowVersion { get; set; } = null!;

        public Usuario Usuario { get; set; } = null!;
        public Asignacion? Asignacion { get; set; }
        public ICollection<EnvioEstadoHistorial> Historial { get; set; } = new List<EnvioEstadoHistorial>();
    }
}
