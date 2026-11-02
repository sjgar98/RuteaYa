using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IDashboardServicio
    {
        Task<DashboardDto> ObtenerAsync(int usuarioId);
    }
}
