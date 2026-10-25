using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IEnvioEstadoHistorialRepositorio : IRepositorio<EnvioEstadoHistorial>
    {
        Task<List<EnvioEstadoHistorial>> ObtenerPorEnvioAsync(int envioId);
    }
}
