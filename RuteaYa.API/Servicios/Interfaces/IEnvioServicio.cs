using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IEnvioServicio
    {
        Task<List<EnvioDto>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado);
        Task<EnvioDetalleDto?> ObtenerDetalleAsync(int usuarioId, int id);
        Task<EnvioDto> CrearAsync(int usuarioId, CrearEnvioDto dto);
        Task<ResultadoOperacion> CancelarAsync(int usuarioId, int id);
    }
}
