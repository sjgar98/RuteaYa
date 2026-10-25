using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs
{
    // Resultado de la búsqueda de direcciones (servicio de mapas) y parada intermedia de un viaje.
    public class DireccionDto
    {
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}
