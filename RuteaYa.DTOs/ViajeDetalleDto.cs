namespace RuteaYa.DTOs
{
    public class ViajeDetalleDto : ViajeDto
    {
        public decimal OrigenLat { get; set; }
        public decimal OrigenLng { get; set; }
        public decimal DestinoLat { get; set; }
        public decimal DestinoLng { get; set; }

        // En el orden del recorrido.
        public List<DireccionDto> Paradas { get; set; } = new List<DireccionDto>();
        public List<PaqueteAsignadoDto> PaquetesAsignados { get; set; } = new List<PaqueteAsignadoDto>();
    }
}
