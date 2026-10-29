using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface IOportunidadesServicio
    {
        /// <summary>
        /// Obtiene la lista de oportunidades (envíos compatibles) para un viaje.
        /// </summary>
        /// <param name="viajeId">ID del viaje</param>
        /// <param name="ordenarPor">Criterio de ordenamiento: "ganancia" (default), "desvio" o "compatibilidad"</param>
        Task<List<OportunidadDto>> ObtenerOportunidadesAsync(int viajeId, string? ordenarPor = "ganancia");

        /// <summary>
        /// Obtiene el detalle completo de una oportunidad (envío + información de ruta).
        /// </summary>
        /// <param name="viajeId">ID del viaje</param>
        /// <param name="envioId">ID del envío (oportunidad)</param>
        /// <returns>Null si no existe o pertenece a otro usuario</returns>
        Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int viajeId, int envioId);

        /// <summary>
        /// Acepta una oportunidad: asigna el envío al viaje del usuario.
        /// </summary>
        /// <param name="viajeId">ID del viaje</param>
        /// <param name="envioId">ID del envío a aceptar</param>
        /// <returns>Resultado de la operación con mensajes de error si aplica</returns>
        Task<ResultadoAceptarOportunidad> AceptarAsync(int viajeId, int envioId);
    }

    // Modelo auxiliar para encapsular resultado de aceptación
    public class ResultadoAceptarOportunidad
    {
        public bool Exitoso { get; set; }
        public bool NoEncontrado { get; set; }
        public string? Conflicto { get; set; }
    }
}
