using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IAsignacionRepositorio
    {
        Task<List<Asignacion>> ListarPorConductorAsync(int conductorId);
        Task<Asignacion?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(List<Asignacion> asignaciones);
        Task GuardarCambiosAsync();  
    }
}