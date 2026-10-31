using RuteaYa.DTOs;
using RuteaYa.MVC.Models;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IOportunidadesServicio
    {
        // null si el viaje no existe o no es del usuario (404 de la API).
        Task<OportunidadesViajeDto?> ListarAsync(int viajeId, string ordenarPor);

        // null si el viaje no es del usuario o el envío ya no está disponible (404 de la API).
        Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int viajeId, int envioId);

        Task<ResultadoAceptarOportunidad> AceptarAsync(int viajeId, int envioId);
    }
}
