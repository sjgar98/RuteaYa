using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs.Rutas
{
    // Recorrido a calcular: origen, paradas intermedias y destino, en ese orden.
    public class ConsultaRutaDto
    {
        [Required(ErrorMessage = "El recorrido es obligatorio.")]
        [MinLength(2, ErrorMessage = "El recorrido necesita al menos origen y destino.")]
        [MaxLength(25, ErrorMessage = "El recorrido admite hasta 25 puntos.")]
        public List<CoordenadaDto> Recorrido { get; set; } = new List<CoordenadaDto>();
    }
}