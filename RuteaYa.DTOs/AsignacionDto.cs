namespace RuteaYa.DTOs
{
    // Envío asignado a un viaje al aceptar una oportunidad.
    public class AsignacionDto
    {
        public int Id { get; set; }
        public int EnvioId { get; set; }
        public int ViajeId { get; set; }
        public DateOnly FechaViaje { get; set; }
        public decimal DesvioKm { get; set; }
        public int TiempoExtraMin { get; set; }
        public decimal Ganancia { get; set; }
    }
}
