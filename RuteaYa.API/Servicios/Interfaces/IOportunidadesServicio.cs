using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IOportunidadesServicio
    {
        Task<List<OportunidadDto>> ObtenerOportunidadesAsync(int viajeId, string? ordenarPor = "ganancia");
        Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int viajeId, int envioId);
        Task<ResultadoOperacion> AceptarAsync(int viajeId, int envioId);
    }
}
