using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IOptimizadorServicio
    {
        // null si el viaje no existe o no es del usuario.
        // criterio: "ganancia" (default), "desvio" o "equilibrio".
        Task<OptimizacionResultadoDto?> OptimizarAsync(int usuarioId, int viajeId, string? criterio = "ganancia");
    }
}

