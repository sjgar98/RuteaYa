using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IPotencialServicio
    {
        Task<PotencialDto> CalcularPotencialMensualAsync(int usuarioId, string? mes, string? criterio);
    }
}