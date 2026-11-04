using RuteaYa.DTOs;
using RuteaYa.Entidades;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IOptimizadorServicio
    {
        // null si el viaje no existe o no es del usuario.
        // criterio: "ganancia" (default), "desvio" o "equilibrio".
        Task<OptimizacionResultadoDto?> OptimizarAsync(int usuarioId, int viajeId, string? criterio = "ganancia");

        Task<OptimizacionResultadoDto> OptimizarViajeAsync(
            Viaje viaje,
            int usuarioId,
            string? criterio,
            DateOnly desde,
            DateOnly hasta,
            IReadOnlySet<int> enviosExcluidos);
    }
}

