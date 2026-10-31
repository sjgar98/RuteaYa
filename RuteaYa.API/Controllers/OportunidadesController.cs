using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Extensiones;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OportunidadesController : ControllerBase
    {
        private readonly IOportunidadesServicio _oportunidadesServicio;

        public OportunidadesController(IOportunidadesServicio oportunidadesServicio)
        {
            _oportunidadesServicio = oportunidadesServicio;
        }

        /// <summary>
        /// Lista los envíos compatibles (oportunidades) con un viaje del usuario autenticado: disponibles,
        /// de otros usuarios, con una salida del viaje antes de su fecha límite que tenga lugar
        /// (paquetes y peso) y con un desvío dentro del máximo del viaje.
        /// </summary>
        /// <param name="viajeId">ID del viaje.</param>
        /// <param name="ordenarPor">"ganancia" (default), "desvio" o "compatibilidad". Los empates se ordenan por la otra métrica.</param>
        /// <response code="200">Datos del viaje y oportunidades ordenadas (vacía si el viaje está inactivo).</response>
        /// <response code="404">El viaje no existe o pertenece a otro usuario.</response>
        [HttpGet("{viajeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OportunidadesViajeDto>> Listar(int viajeId, [FromQuery] string? ordenarPor = "ganancia")
        {
            var resultado = await _oportunidadesServicio.ListarAsync(User.ObtenerUsuarioId(), viajeId, ordenarPor);
            if (resultado == null)
            {
                return NotFound();
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Detalle de una oportunidad: ruta habitual vs. con el envío, desvío, tiempos, pago, ganancia por km
        /// adicional y capacidad restante. Si las restricciones dejaron de cumplirse, PuedeAceptar es false
        /// y Motivo explica por qué.
        /// </summary>
        /// <param name="viajeId">ID del viaje.</param>
        /// <param name="envioId">ID del envío.</param>
        /// <response code="200">Detalle de la oportunidad.</response>
        /// <response code="404">El viaje no es del usuario, o el envío no existe, es propio o ya no busca conductor.</response>
        [HttpGet("{viajeId:int}/detalle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DetalleOportunidadDto>> Detalle(int viajeId, [FromQuery] int envioId)
        {
            var detalle = await _oportunidadesServicio.ObtenerDetalleAsync(User.ObtenerUsuarioId(), viajeId, envioId);
            if (detalle == null)
            {
                return NotFound();
            }

            return Ok(detalle);
        }

        /// <summary>
        /// Acepta una oportunidad: revalida las restricciones, asigna el envío al viaje en la primera salida
        /// con lugar y pasa el envío a "Conductor asignado".
        /// </summary>
        /// <param name="viajeId">ID del viaje.</param>
        /// <param name="dto">Envío a aceptar.</param>
        /// <response code="201">Envío asignado.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="404">El viaje no es del usuario o el envío no existe.</response>
        /// <response code="409">Las restricciones ya no se cumplen o el envío fue tomado por otro conductor (el motivo va en title).</response>
        [HttpPost("{viajeId:int}/aceptar")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AsignacionDto>> Aceptar(int viajeId, [FromBody] AceptarOportunidadDto dto)
        {
            var resultado = await _oportunidadesServicio.AceptarAsync(User.ObtenerUsuarioId(), viajeId, dto.EnvioId!.Value);

            return resultado.Resultado switch
            {
                ResultadoOperacion.NoEncontrado => NotFound(),
                ResultadoOperacion.Conflicto => Problem(statusCode: StatusCodes.Status409Conflict, title: resultado.Mensaje),
                _ => StatusCode(StatusCodes.Status201Created, resultado.Asignacion)
            };
        }
    }
}
