using RuteaYa.Entidades;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IEnvioRepositorio
    {
        Task<List<Envio>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null);
        Task<List<Envio>> ListarDisponiblesAsync(DateOnly desde, int usuarioIdExcluido);
        Task<List<Envio>> ListarPorIdsAsync(List<int> ids);
        Task<Envio?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Envio envio);
        Task GuardarCambiosAsync();
    }
}
