using RuteaYa.DTOs.Enums;

namespace RuteaYa.DTOs
{
    public class TransporteDto
    {
        public int AsignacionId { get; set; }
        public int EnvioId { get; set; }
        public int ViajeId { get; set; }

        // Datos del paquete (envío).
        public string Descripcion { get; set; } = string.Empty;
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public EstadoEnvio Estado { get; set; }
        public decimal Ganancia { get; set; }

        // Datos del viaje asociado, para el link "Ver viaje".
        public DateOnly FechaViaje { get; set; }
        public string ViajeOrigenDir { get; set; } = string.Empty;
        public string ViajeDestinoDir { get; set; } = string.Empty;

        // Calculados con EnvioEstadoMaquina: la vista no decide por sí sola qué acción mostrar.
        public bool PuedeMarcarRetirado { get; set; }
        public bool PuedeMarcarEnCamino { get; set; }
        public bool PuedeMarcarEntregado { get; set; }
    }
}