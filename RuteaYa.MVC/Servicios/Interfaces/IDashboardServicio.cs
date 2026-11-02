using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IDashboardServicio
    {
        Task<DashboardDto> ObtenerAsync();
    }
}
