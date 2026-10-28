using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface ITransporteServicio
    {
        Task<List<TransporteDto>> ListarAsync(int conductorId);
        Task<ResultadoOperacion> MarcarRetiradoAsync(int conductorId, int asignacionId);
        Task<ResultadoOperacion> MarcarEnCaminoAsync(int conductorId, int asignacionId);
        Task<ResultadoOperacion> MarcarEntregadoAsync(int conductorId, int asignacionId);
    }
}