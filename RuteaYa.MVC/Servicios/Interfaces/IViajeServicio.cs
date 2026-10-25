using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IViajeServicio
    {
        Task<List<ViajeDto>> ListarAsync(bool? activo);
    }
}
