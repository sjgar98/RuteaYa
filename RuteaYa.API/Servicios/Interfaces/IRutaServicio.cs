using System.Collections.Generic;
using System.Threading.Tasks;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IRutaServicio
    {
        Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion);
        Task<RutaResultadoDto> CalcularRutaConParadasAsync(List<string> paradas);
        Task<RutaResultadoDto> CalcularRutaAlternativaEnvioAsync(string origen, string destino, string retiroPaquete, string entregaPaquete);
        Task<decimal> CalcularDesvioAsync(string rutaOriginalId, string paradaExtra);
        Task<DetalleOportunidadRutaDto> ObtenerDetalleOportunidadAsync(string origenHabitual, string destinoHabitual, string retiroEnvio, string entregaEnvio);
        Task<List<string>> BuscarDireccionesAsync(string texto);
    }
}