using System.ComponentModel.DataAnnotations;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Validaciones;

namespace RuteaYa.DTOs
{
    public class CrearEnvioDto
    {
        [Required(ErrorMessage = "El origen es obligatorio.")]
        public string OrigenDir { get; set; } = string.Empty;

        // Las coordenadas se completan al elegir una dirección de la búsqueda.
        [Required(ErrorMessage = "Seleccioná un origen de la lista.")]
        public decimal? OrigenLat { get; set; }

        [Required(ErrorMessage = "Seleccioná un origen de la lista.")]
        public decimal? OrigenLng { get; set; }

        [Required(ErrorMessage = "El destino es obligatorio.")]
        [DistintoDe(nameof(OrigenDir), ErrorMessage = "El destino debe ser distinto del origen.")]
        public string DestinoDir { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná un destino de la lista.")]
        public decimal? DestinoLat { get; set; }

        [Required(ErrorMessage = "Seleccioná un destino de la lista.")]
        public decimal? DestinoLng { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [FechaNoAnteriorAHoy(ErrorMessage = "La fecha no puede ser anterior a hoy.")]
        public DateOnly? FechaLimite { get; set; }

        [Required(ErrorMessage = "El peso es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El peso debe ser mayor que cero.")]
        public decimal? PesoKg { get; set; }

        [Required(ErrorMessage = "Seleccioná un tamaño.")]
        public TamanoEnvio? Tamano { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto ofrecido es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto ofrecido debe ser mayor que cero.")]
        public decimal? Monto { get; set; }
    }
}