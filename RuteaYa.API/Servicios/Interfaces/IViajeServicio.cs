using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IViajeServicio
    {
        Task<List<ViajeDto>> ListarPorUsuarioAsync(int usuarioId, bool? activo);
    }
}
