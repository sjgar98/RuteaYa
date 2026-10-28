using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.MVC.Models
{
    public class MisEnviosViewModel
    {
        public List<EnvioDto> Envios { get; set; } = new List<EnvioDto>();

        public EstadoEnvio? Estado { get; set; }
    }
}