using RuteaYa.DTOs;

namespace RuteaYa.MVC.Models
{
    public class OportunidadesViewModel
    {
        // Viajes activos del usuario, para elegir sobre cuál buscar oportunidades.
        public List<ViajeDto> Viajes { get; set; } = new List<ViajeDto>();

        // Viaje seleccionado y sus oportunidades; null si el usuario no tiene viajes activos.
        public OportunidadesViajeDto? Viaje { get; set; }

        // "ganancia", "desvio" o "compatibilidad".
        public string OrdenarPor { get; set; } = "ganancia";
    }
}
