using RuteaYa.Entidades;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IParadaRepositorio : IRepositorio<Parada>
    {
        Task<List<Parada>> ObtenerPorViajeAsync(int viajeId);
        Task<Parada?> ObtenerDeViajeAsync(int viajeId, int paradaId);
    }
}
