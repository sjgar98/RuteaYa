namespace RuteaYa.DTOs
{
    public class ViajeDetalleDto : ViajeDto
    {
        // En el orden del recorrido.
        public List<DireccionDto> Paradas { get; set; } = new List<DireccionDto>();
        public List<PaqueteAsignadoDto> PaquetesAsignados { get; set; } = new List<PaqueteAsignadoDto>();
    }
}
