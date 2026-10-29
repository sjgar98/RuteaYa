using RuteaYa.DTOs.Rutas;

namespace RuteaYa.DTOs
{
    public class DetalleOportunidadDto
    {
        public int EnvioId { get; set; }
        public int ViajeId { get; set; }
        public EnvioDto Envio { get; set; } = new EnvioDto();
        public DetalleOportunidadRutaDto RutaDetalle { get; set; } = new DetalleOportunidadRutaDto();
    }
}
