namespace RuteaYa.Entidades
{
    public class Asignacion
    {
        public int Id { get; set; }
        public int EnvioId { get; set; }
        public int ViajeId { get; set; }
        public DateOnly FechaViaje { get; set; }
        public decimal DesvioKm { get; set; }
        public int TiempoExtraMin { get; set; }
        public decimal Ganancia { get; set; }
        public DateTime AsignadoEn { get; set; }

        public Envio Envio { get; set; } = null!;
        public Viaje Viaje { get; set; } = null!;
    }
}
