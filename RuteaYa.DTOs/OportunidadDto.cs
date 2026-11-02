namespace RuteaYa.DTOs
{
    public class OportunidadDto
    {
        public int EnvioId { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PesoKg { get; set; }
        public decimal DesvioKm { get; set; }
        public decimal MontoPago { get; set; }

        // Primera fecha en que el viaje sale antes de la fecha límite del envío.
        public DateOnly FechaViaje { get; set; }

        // "Muy compatible", "Compatible" o "Mucho desvío", según el desvío respecto del máximo del viaje.
        public string Compatibilidad { get; set; } = string.Empty;

        // 0 a 1: 1 es sin desvío, 0 es el desvío máximo del viaje.
        public decimal PuntuacionCompatibilidad { get; set; }

        // Lugar que le queda al viaje antes de aceptar este envío.
        public int CapacidadRestantePaquetes { get; set; }
        public decimal CapacidadRestanteKg { get; set; }
    }
}
