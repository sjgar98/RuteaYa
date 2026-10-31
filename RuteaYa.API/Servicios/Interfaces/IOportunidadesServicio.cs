using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IOportunidadesServicio
    {
        // null si el viaje no existe o no es del usuario.
        Task<OportunidadesViajeDto?> ListarAsync(int usuarioId, int viajeId, string? ordenarPor = "ganancia");

        // null si el viaje no es del usuario, o si el envío no existe, es propio o ya no busca conductor.
        Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int usuarioId, int viajeId, int envioId);

        // Revalida todas las restricciones, asigna el envío al viaje y lo pasa a ConductorAsignado.
        Task<ResultadoAsignacion> AceptarAsync(int usuarioId, int viajeId, int envioId);
    }
}
