using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IHistorialServicio
    {
        Task<HistorialDto> ObtenerHistorialAsync(int usuarioId, DateTime? desde, DateTime? hasta, string? orden, string? estado);
    }
}