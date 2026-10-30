using System.Collections.Generic;
using System.Threading.Tasks;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IRutaServicio
    {

        /// <summary>Ruta por el recorrido completo en orden: origen, paradas, destino.</summary>
        Task<RutaResultadoDto> CalcularRutaAsync(IReadOnlyList<CoordenadaDto> recorrido, CancellationToken cancelacion = default);

        /// <summary>
        /// Compara el recorrido habitual con el que incluye retiro y entrega del envío (el servicio elige
        /// dónde insertarlos; el retiro va antes que la entrega). Completa solo distancias, desvío, tiempos y
        /// las rutas (RutaHabitual / RutaConEnvio); el resto del DTO lo calcula quien lo consume.
        /// </summary>
        Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(
            IReadOnlyList<CoordenadaDto> recorridoHabitual, CoordenadaDto retiro, CoordenadaDto entrega, CancellationToken cancelacion = default);

        Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion);

        // TODO Servicio de rutas: borrar cuando ViajeServicio y OportunidadesServicio usen coordenadas.
        Task<RutaResultadoDto> CalcularRutaConParadasAsync(List<string> paradas);
        Task<RutaResultadoDto> CalcularRutaAlternativaEnvioAsync(string origen, string destino, string retiroPaquete, string entregaPaquete);
        Task<decimal> CalcularDesvioAsync(string rutaOriginalId, string paradaExtra);
        Task<DetalleOportunidadRutaDto> ObtenerDetalleOportunidadAsync(string origenHabitual, string destinoHabitual, string retiroEnvio, string entregaEnvio);
        Task<List<string>> BuscarDireccionesAsync(string texto);
    }
}