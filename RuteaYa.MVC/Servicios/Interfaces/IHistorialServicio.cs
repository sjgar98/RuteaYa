using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IHistorialServicio
    {
        // null si la API rechaza los filtros (400: desde posterior a hasta).
        Task<HistorialDto?> ObtenerHistorialAsync(DateOnly? desde, DateOnly? hasta, string? orden, string? estado);
    }
}
