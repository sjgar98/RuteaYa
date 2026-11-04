using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Extensiones;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PotencialController : ControllerBase
    {
        private readonly IPotencialServicio _potencialServicio;

        public PotencialController(IPotencialServicio potencialServicio)
        {
            _potencialServicio = potencialServicio;
        }

        /// <summary>
        /// Estima la ganancia potencial de un mes aplicando el optimizador a cada viaje activo del usuario
        /// con salidas en el mes (desde hoy si es el mes actual), sin repetir un envío entre viajes.
        /// </summary>
        /// <param name="mes">Mes a analizar con formato yyyy-MM.</param>
        /// <param name="criterio">"ganancia" (default), "desvio" o "equilibrio".</param>
        /// <response code="200">Resumen del potencial mensual con el detalle por viaje y por envío.</response>
        /// <response code="400">El mes no tiene el formato yyyy-MM.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PotencialMensualDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PotencialMensualDto>> Obtener(
            [FromQuery] string mes,
            [FromQuery] string? criterio = "ganancia")
        {
            var usuarioId = User.ObtenerUsuarioId();
            var resultado = await _potencialServicio.ObtenerPotencialAsync(usuarioId, mes, criterio);

            if (resultado == null)
            {
                return Problem(title: "El mes debe tener el formato yyyy-MM.", statusCode: StatusCodes.Status400BadRequest);
            }

            return Ok(resultado);
        }
    }
}