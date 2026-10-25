using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IViajeRepositorio
    {
        Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null);
        Task<Viaje?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Viaje viaje);
        Task GuardarCambiosAsync();
    }
}
