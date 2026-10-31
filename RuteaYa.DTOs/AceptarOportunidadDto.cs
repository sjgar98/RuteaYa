using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs
{
    public class AceptarOportunidadDto
    {
        [Required(ErrorMessage = "Seleccioná el envío a aceptar.")]
        public int? EnvioId { get; set; }
    }
}
