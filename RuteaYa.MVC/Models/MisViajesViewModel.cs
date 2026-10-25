using RuteaYa.DTOs;

namespace RuteaYa.MVC.Models
{
    public class MisViajesViewModel
    {
        public List<ViajeDto> Viajes { get; set; } = new List<ViajeDto>();

        // Filtro seleccionado: true activos, false inactivos, null todos.
        public bool? Activo { get; set; }
    }
}
