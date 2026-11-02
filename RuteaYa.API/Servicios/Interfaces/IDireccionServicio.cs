using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IDireccionServicio
    {
        // Direcciones que coinciden con el texto, ya con coordenadas (origen, destino y paradas).
        Task<List<DireccionDto>> BuscarAsync(string texto, CancellationToken cancelacion = default);
    }
}