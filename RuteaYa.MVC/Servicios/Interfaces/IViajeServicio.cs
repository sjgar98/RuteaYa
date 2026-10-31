using RuteaYa.DTOs;
using RuteaYa.MVC.Models;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IViajeServicio
    {
        Task<List<ViajeDto>> ListarAsync(bool? activo);

        // null si el viaje no existe o no es del usuario (404 de la API).
        Task<ViajeDetalleDto?> ObtenerDetalleAsync(int id);

        // Devuelve los errores de validación de la API por campo (vacío si se creó).
        Task<Dictionary<string, string[]>> CrearAsync(CrearViajeDto dto);

        Task<ResultadoEdicionViaje> ActualizarAsync(int id, CrearViajeDto dto);

        Task<ResultadoCambioEstado> DesactivarAsync(int id);

        Task<ResultadoCambioEstado> ActivarAsync(int id);

        Task<List<DireccionDto>> BuscarDireccionesAsync(string texto);
    }
}
