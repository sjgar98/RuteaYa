using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs
{
    public class ConfirmarSeleccionDto
    {
        [Required(ErrorMessage = "Seleccioná el viaje.")]
        public int? ViajeId { get; set; }

        [Required(ErrorMessage = "Seleccioná al menos un envío.")]
        [MinLength(1, ErrorMessage = "Seleccioná al menos un envío.")]
        public List<int> EnvioIds { get; set; } = new List<int>();
    }
}
