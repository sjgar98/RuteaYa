using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IViajeServicio
    {
        Task<List<ViajeDto>> ListarPorUsuarioAsync(int usuarioId, bool? activo);
        // null si el viaje no existe o no es del usuario.
        Task<ViajeDetalleDto?> ObtenerDetalleAsync(int usuarioId, int id);
        Task<ViajeDto> CrearAsync(int usuarioId, CrearViajeDto dto);
        // NoEncontrado si no existe o no es del usuario; Conflicto si tiene envíos pendientes.
        Task<ResultadoOperacion> ActualizarAsync(int usuarioId, int id, CrearViajeDto dto);
        // NoEncontrado si no existe o no es del usuario; Conflicto si se desactiva con envíos pendientes.
        Task<ResultadoOperacion> CambiarActivoAsync(int usuarioId, int id, bool activo);
    }
}
