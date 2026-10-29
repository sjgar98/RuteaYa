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
        /// Lista los envíos compatibles (oportunidades) para un viaje del usuario autenticado.
        /// </summary>
        /// <param name="viajeId">ID del viaje</param>
        /// <param name="ordenarPor">Criterio de ordenamiento: "ganancia" (default), "desvio" o "compatibilidad"</param>
        /// <response code="200">Lista de oportunidades ordenadas</response>
        /// <response code="404">El viaje no existe o pertenece a otro usuario</response>
        /// <response code="401">No autenticado</response>
        [HttpGet("{viajeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<OportunidadDto>>> Listar(int viajeId, [FromQuery] string? ordenarPor = "ganancia")
        {
            var usuarioId = User.ObtenerUsuarioId();

            var oportunidades = await _oportunidadesServicio.ObtenerOportunidadesAsync(viajeId, ordenarPor);

            return Ok(oportunidades);
        }

        /// <summary>
        /// Obtiene el detalle completo de una oportunidad (envío compatible con un viaje).
        /// Incluye información de la ruta habitual vs. con el envío.
        /// </summary>
        /// <param name="viajeId">ID del viaje</param>
        /// <param name="envioId">ID del envío</param>
        /// <response code="200">Detalle de la oportunidad</response>
        /// <response code="404">El viaje o envío no existen</response>
        /// <response code="401">No autenticado</response>
        [HttpGet("{viajeId:int}/detalle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DetalleOportunidadDto>> Detalle(int viajeId, [FromQuery] int envioId)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var detalle = await _oportunidadesServicio.ObtenerDetalleAsync(viajeId, envioId);
            if (detalle == null)
            {
                return NotFound();
            }

            return Ok(detalle);
        }

        /// <summary>
        /// Acepta una oportunidad: asigna un envío a un viaje del usuario autenticado.
        /// Crea la asignación en la base de datos.
        /// </summary>
        /// <param name="viajeId">ID del viaje</param>
        /// <param name="dto">DTO con el ID del envío a aceptar</param>
        /// <response code="201">Envío asignado correctamente</response>
        /// <response code="400">Datos inválidos o conflicto de capacidad/desvío</response>
        /// <response code="404">El viaje o envío no existen</response>
        /// <response code="409">El envío ya fue asignado o no está disponible</response>
        /// <response code="401">No autenticado</response>
        [HttpPost("{viajeId:int}/aceptar")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Aceptar(int viajeId, [FromBody] AceptarOportunidadDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!dto.EnvioId.HasValue)
            {
                return BadRequest(new { mensaje = "El ID del envío es requerido." });
            }

            var usuarioId = User.ObtenerUsuarioId();

            var resultado = await _oportunidadesServicio.AceptarAsync(viajeId, dto.EnvioId.Value);

            return resultado switch
            {
                ResultadoOperacion.NoEncontrado => NotFound(),
                ResultadoOperacion.Conflicto => Conflict(new { mensaje = "No se pudo aceptar la oportunidad: capacidad insuficiente, desvío excedido o envío ya asignado." }),
                ResultadoOperacion.Exito => CreatedAtAction(nameof(Detalle), new { viajeId, envioId = dto.EnvioId }, null),
                _ => BadRequest(new { mensaje = "Error desconocido al procesar la solicitud." })
            };
        }
    }
}
