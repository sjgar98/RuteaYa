using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IHistorialServicio
    {
        Task<HistorialDto?> ObtenerHistorialAsync(DateTime? desde, DateTime? hasta, string? orden,string? estado);
    }
}