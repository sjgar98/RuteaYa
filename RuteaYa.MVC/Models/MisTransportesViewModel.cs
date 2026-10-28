using RuteaYa.DTOs;

namespace RuteaYa.MVC.Models
{
    public class MisTransportesViewModel
    {
        public List<TransporteDto> Transportes { get; set; } = new List<TransporteDto>();
        public string? Error { get; set; }
    }
}