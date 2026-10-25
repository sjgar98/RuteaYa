using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<bool> ExisteEmailAsync(string email);
        Task AgregarAsync(Usuario usuario);
    }
}
