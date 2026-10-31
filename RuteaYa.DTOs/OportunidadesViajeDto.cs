namespace RuteaYa.DTOs
{
    // Oportunidades de un viaje junto con los datos del viaje que la pantalla necesita mostrar.
    public class OportunidadesViajeDto
    {
        public int ViajeId { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int MaxPaquetes { get; set; }
        public decimal PesoMaxKg { get; set; }
        public decimal DesvioMaxKm { get; set; }
        public List<OportunidadDto> Oportunidades { get; set; } = new List<OportunidadDto>();
    }
}
