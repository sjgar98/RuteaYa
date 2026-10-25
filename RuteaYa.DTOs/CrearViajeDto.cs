using System.ComponentModel.DataAnnotations;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Validaciones;

namespace RuteaYa.DTOs
{
    public class CrearViajeDto
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

        // Opcionales. El orden de la lista es el orden del recorrido.
        [DireccionesUnicas(nameof(OrigenDir), nameof(DestinoDir),
            ErrorMessage = "Las paradas no pueden repetirse ni coincidir con el origen o el destino.")]
        public List<DireccionDto> Paradas { get; set; } = new List<DireccionDto>();

        [Required(ErrorMessage = "Seleccioná al menos un día.")]
        [MinLength(1, ErrorMessage = "Seleccioná al menos un día.")]
        public List<DiaSemana> Dias { get; set; } = new List<DiaSemana>();

        [Required(ErrorMessage = "La hora de salida es obligatoria.")]
        public TimeOnly? HoraSalida { get; set; }

        // Salida y llegada son del mismo día.
        [Required(ErrorMessage = "La hora de llegada es obligatoria.")]
        [MayorQue(nameof(HoraSalida), ErrorMessage = "La hora de llegada debe ser posterior a la de salida.")]
        public TimeOnly? HoraLlegada { get; set; }

        [Required(ErrorMessage = "La cantidad máxima de paquetes es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad máxima de paquetes debe ser mayor que cero.")]
        public int? MaxPaquetes { get; set; }

        [Required(ErrorMessage = "El peso máximo es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El peso máximo debe ser mayor que cero.")]
        public decimal? PesoMaxKg { get; set; }

        [Required(ErrorMessage = "El desvío máximo es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El desvío máximo debe ser mayor que cero.")]
        public decimal? DesvioMaxKm { get; set; }
    }
}
