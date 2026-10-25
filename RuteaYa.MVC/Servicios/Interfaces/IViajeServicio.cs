using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IViajeServicio
    {
        Task<List<ViajeDto>> ListarAsync(bool? activo);

        // Devuelve los errores de validación de la API por campo (vacío si se creó).
        Task<Dictionary<string, string[]>> CrearAsync(CrearViajeDto dto);

        Task<List<DireccionDto>> BuscarDireccionesAsync(string texto);
    }
}
