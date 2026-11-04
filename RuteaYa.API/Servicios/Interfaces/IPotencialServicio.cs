using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IPotencialServicio
    {
        // Estima el potencial mensual combinando los viajes activos del usuario y los envíos disponibles en el mes.
        Task<PotencialMensualDto?> ObtenerPotencialAsync(int usuarioId, string mes, string? criterio = "ganancia");
    }
}