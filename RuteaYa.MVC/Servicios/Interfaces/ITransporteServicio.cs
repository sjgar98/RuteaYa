using RuteaYa.DTOs;
using RuteaYa.MVC.Models;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface ITransporteServicio
    {
        Task<List<TransporteDto>> ListarAsync();
        Task<EnvioDetalleDto?> ObtenerEnvioAsync(int asignacionId);
        Task<ResultadoCambioEstado> MarcarRetiradoAsync(int asignacionId);
        Task<ResultadoCambioEstado> MarcarEnCaminoAsync(int asignacionId);
        Task<ResultadoCambioEstado> MarcarEntregadoAsync(int asignacionId);
    }
}