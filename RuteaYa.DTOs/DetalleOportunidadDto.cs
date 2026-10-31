using RuteaYa.DTOs.Rutas;

namespace RuteaYa.DTOs
{
    public class DetalleOportunidadDto
    {
        public int EnvioId { get; set; }
        public int ViajeId { get; set; }
        public string ViajeOrigenDir { get; set; } = string.Empty;
        public string ViajeDestinoDir { get; set; } = string.Empty;
        public EnvioDto Envio { get; set; } = new EnvioDto();
        public DetalleOportunidadRutaDto RutaDetalle { get; set; } = new DetalleOportunidadRutaDto();

        // Fecha en la que se llevaría el envío; null si el viaje no tiene una fecha válida con lugar.
        public DateOnly? FechaViaje { get; set; }

        public string Compatibilidad { get; set; } = string.Empty;

        // Si las restricciones dejaron de cumplirse, no se puede aceptar y Motivo explica por qué.
        public bool PuedeAceptar { get; set; }
        public string? Motivo { get; set; }
    }
}
