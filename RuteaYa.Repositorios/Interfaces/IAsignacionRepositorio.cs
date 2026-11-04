using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IAsignacionRepositorio
    {
        Task<List<Asignacion>> ListarPorConductorAsync(int conductorId);
        Task<List<Asignacion>> ListarEntregadasPorConductorAsync(int conductorId, DateOnly? desde, DateOnly? hasta);
        Task<Asignacion?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(List<Asignacion> asignaciones);
        Task GuardarCambiosAsync();
    }
}