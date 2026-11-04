using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IHistorialServicio
    {
        // estado: "viajes", "entregados", "cancelados" o null/"todos". orden: "fecha_desc" (default), "fecha_asc", "monto_desc" o "monto_asc".
        Task<HistorialDto> ObtenerHistorialAsync(int usuarioId, DateOnly? desde, DateOnly? hasta, string? orden, string? estado);
    }
}
