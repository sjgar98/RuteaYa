using RuteaYa.Entidades;
using RuteaYa.Entidades.Enums;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IAsignacionRepositorio : IRepositorio<Asignacion>
    {
        Task<List<Asignacion>> ObtenerPorViajeAsync(int viajeId);

        /// <summary>Asignaciones de los viajes de un conductor (pantalla "Mis transportes" e "Historial").</summary>
        Task<List<Asignacion>> ObtenerPorConductorAsync(
            int conductorId,
            IEnumerable<EstadoEnvio>? estados = null,
            DateOnly? desde = null,
            DateOnly? hasta = null);

        Task<Asignacion?> ObtenerConDetalleAsync(int id);
        Task<bool> ExisteParaEnvioAsync(int envioId);
    }
}
