using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IOptimizadorServicio
    {
        // null si el viaje no existe o no es del usuario (404 de la API).
        Task<OptimizacionResultadoDto?> OptimizarAsync(int viajeId, string criterio);
    }
}

