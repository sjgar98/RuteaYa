using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IViajeServicio
    {
        Task<List<ViajeDto>> ListarPorUsuarioAsync(int usuarioId, bool? activo);
        Task<ViajeDto> CrearAsync(int usuarioId, CrearViajeDto dto);
        Task<List<DireccionDto>> BuscarDireccionesAsync(string texto);
    }
}
