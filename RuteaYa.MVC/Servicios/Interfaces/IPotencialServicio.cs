using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IPotencialServicio
    {
       
        Task<PotencialMensualDto?> ObtenerPotencialAsync(string mes, string criterio);
    }
}