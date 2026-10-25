using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IViajeRepositorio : IRepositorio<Viaje>
    {
        /// <param name="activo">null devuelve todos (activos e inactivos).</param>
        Task<List<Viaje>> ObtenerPorUsuarioAsync(int usuarioId, bool? activo = null);

        /// <summary>Viaje con paradas (ordenadas), días y asignaciones con su envío.</summary>
        Task<Viaje?> ObtenerConDetalleAsync(int id);

        /// <summary>Devuelve el viaje con detalle solo si pertenece al usuario.</summary>
        Task<Viaje?> ObtenerDeUsuarioAsync(int id, int usuarioId);
    }
}
