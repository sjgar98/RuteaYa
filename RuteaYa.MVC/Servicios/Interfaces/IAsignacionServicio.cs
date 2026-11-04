using RuteaYa.DTOs;
using RuteaYa.MVC.Models;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IAsignacionServicio
    {
        // null si el viaje no es del usuario o algún envío no existe (404 de la API).
        Task<ResumenSeleccionDto?> ObtenerResumenAsync(int viajeId, List<int> envioIds);

        Task<ResultadoConfirmacion> ConfirmarAsync(int viajeId, List<int> envioIds);
    }
}
