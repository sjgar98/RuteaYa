using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IPotencialServicio
    {
        Task<PotencialDto?> ObtenerPotencialAsync(string? mes, string? criterio);
    }
}