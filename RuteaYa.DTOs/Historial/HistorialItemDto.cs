using RuteaYa.DTOs.Enums;

namespace RuteaYa.DTOs
{
    // Un ítem del historial: un viaje realizado en un mes (Tipo "Viaje", Id del viaje) o un envío propio
    // que terminó (Tipo "Envio", Id del envío, con su Estado).
    public class HistorialItemDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateOnly Fecha { get; set; }
        public EstadoEnvio? Estado { get; set; }
    }
}
