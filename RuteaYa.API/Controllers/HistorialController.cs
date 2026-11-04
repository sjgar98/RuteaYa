using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Extensiones;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/historial")]
    public class HistorialController : ControllerBase
    {
        private readonly IHistorialServicio _historialServicio;

        public HistorialController(IHistorialServicio historialServicio)
        {
            _historialServicio = historialServicio;
        }

        /// <summary>
        /// Historial del usuario autenticado: viajes realizados como conductor (agrupados por viaje y mes) y
        /// envíos propios finalizados (entregados o cancelados). Los contadores son la ganancia efectiva:
        /// solo cuentan los envíos que el usuario transportó y ya se entregaron.
        /// </summary>
        /// <param name="desde">Fecha mínima (inclusive, yyyy-MM-dd): fecha del viaje o del cierre del envío.</param>
        /// <param name="hasta">Fecha máxima (inclusive, yyyy-MM-dd).</param>
        /// <param name="orden">"fecha_desc" (default), "fecha_asc", "monto_desc" o "monto_asc".</param>
        /// <param name="estado">"todos" (default), "viajes", "entregados" o "cancelados" (estos dos, envíos propios).</param>
        /// <response code="200">Contadores e ítems del historial.</response>
        /// <response code="400">La fecha desde es posterior a la fecha hasta.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<HistorialDto>> ObtenerHistorial(
            [FromQuery] DateOnly? desde,
            [FromQuery] DateOnly? hasta,
            [FromQuery] string? orden,
            [FromQuery] string? estado)
        {
            if (desde.HasValue && hasta.HasValue && desde > hasta)
            {
                ModelState.AddModelError(nameof(desde), "La fecha desde no puede ser posterior a la fecha hasta.");
                return ValidationProblem(ModelState);
            }

            var resultado = await _historialServicio.ObtenerHistorialAsync(User.ObtenerUsuarioId(), desde, hasta, orden, estado);
            return Ok(resultado);
        }
    }
}
