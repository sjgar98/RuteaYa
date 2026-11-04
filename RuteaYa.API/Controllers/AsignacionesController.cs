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
    public class AsignacionesController : ControllerBase
    {
        private readonly IOportunidadesServicio _oportunidadesServicio;

        public AsignacionesController(IOportunidadesServicio oportunidadesServicio)
        {
            _oportunidadesServicio = oportunidadesServicio;
        }

        /// <summary>
        /// Resumen de una selección de envíos para un viaje del usuario antes de reservarla: paquetes, ganancia,
        /// peso y desvío totales. Si algún envío ya no está disponible o la selección supera el lugar libre o el
        /// desvío máximo del viaje, PuedeConfirmar es false y Motivo explica por qué.
        /// </summary>
        /// <param name="viajeId">ID del viaje.</param>
        /// <param name="envioIds">IDs de los envíos seleccionados (repetir el parámetro por cada envío).</param>
        /// <response code="200">Resumen de la selección.</response>
        /// <response code="400">No se indicó ningún envío.</response>
        /// <response code="404">El viaje no es del usuario o algún envío no existe.</response>
        [HttpGet("resumen")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResumenSeleccionDto>> Resumen([FromQuery] int viajeId, [FromQuery] List<int> envioIds)
        {
            if (envioIds.Count == 0)
            {
                ModelState.AddModelError(nameof(envioIds), "Seleccioná al menos un envío.");
                return ValidationProblem(ModelState);
            }

            var resumen = await _oportunidadesServicio.ResumirSeleccionAsync(User.ObtenerUsuarioId(), viajeId, envioIds);
            if (resumen == null)
            {
                return NotFound();
            }

            return Ok(resumen);
        }

        /// <summary>
        /// Confirma y reserva una selección de envíos para un viaje del usuario: revalida todas las restricciones
        /// y asigna todos los envíos en una sola operación (si alguno ya no se puede tomar, no se asigna ninguno).
        /// </summary>
        /// <param name="dto">Viaje y envíos a reservar.</param>
        /// <response code="201">Envíos asignados.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="404">El viaje no es del usuario o algún envío no existe.</response>
        /// <response code="409">Algún envío ya no está disponible o la selección ya no entra en el viaje (el motivo va en title).</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<AsignacionDto>>> Confirmar([FromBody] ConfirmarSeleccionDto dto)
        {
            var resultado = await _oportunidadesServicio.AceptarVariosAsync(User.ObtenerUsuarioId(), dto.ViajeId!.Value, dto.EnvioIds);

            return resultado.Resultado switch
            {
                ResultadoOperacion.NoEncontrado => NotFound(),
                ResultadoOperacion.Conflicto => Problem(statusCode: StatusCodes.Status409Conflict, title: resultado.Mensaje),
                _ => StatusCode(StatusCodes.Status201Created, resultado.Asignaciones)
            };
        }
    }
}
