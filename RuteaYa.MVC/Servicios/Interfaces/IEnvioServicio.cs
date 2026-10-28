using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.MVC.Models;

public interface IEnvioServicio
{
    Task<List<EnvioDto>> ListarAsync(EstadoEnvio? estado);
    Task<EnvioDetalleDto?> ObtenerDetalleAsync(int id);
    Task<Dictionary<string, string[]>> CrearAsync(CrearEnvioDto dto);  
    Task<ResultadoCancelacionEnvio> CancelarAsync(int id);
}