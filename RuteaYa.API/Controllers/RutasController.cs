using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.API.Servicios.Rutas;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RutasController : ControllerBase
    {
        private readonly IRutaServicio _rutaServicio;

        public RutasController(IRutaServicio rutaServicio)
        {
            _rutaServicio = rutaServicio;
        }

        /// <summary>Calcula distancia, duración y trazado de un recorrido (lo usa el mapa de los viajes).</summary>
        /// <param name="consulta">Puntos del recorrido: origen, paradas intermedias y destino, en orden.</param>
        /// <param name="cancelacion">Cancelación de la petición (la completa el framework).</param>
        [HttpPost("calcular")]
        [ProducesResponseType(typeof(RutaResultadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<RutaResultadoDto>> Calcular(
            [FromBody] ConsultaRutaDto consulta, CancellationToken cancelacion)
        {
            try
            {
                var ruta = await _rutaServicio.CalcularRutaAsync(consulta.Recorrido, cancelacion);

                // Se devuelve una copia sin los puntos decodificados: el mapa dibuja con la polyline y los puntos pesan mucho.
                // (La ruta original puede estar en la caché del servicio: no se modifica.)
                return Ok(new RutaResultadoDto
                {
                    DistanciaKm = ruta.DistanciaKm,
                    DuracionMinutos = ruta.DuracionMinutos,
                    Polyline = ruta.Polyline
                });
            }
            catch (Exception ex) when (ex is RutaNoDisponibleException || ex is HttpRequestException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new ProblemDetails { Title = "No se pudo calcular la ruta." });
            }
        }
    }
}