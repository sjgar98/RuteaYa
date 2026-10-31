using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Extensiones;
using RuteaYa.API.Servicios.Simulados;
using RuteaYa.DTOs;

namespace RuteaYa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/potencial")]
    public class PotencialController : ControllerBase
    {
        private readonly IPotencialServicio _potencialServicio;

        public PotencialController(IPotencialServicio potencialServicio)
        {
            _potencialServicio = potencialServicio;
        }

        /// <summary>
        /// Calcula el potencial de ganancias mensual del usuario autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PotencialDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<PotencialDto>> ObtenerPotencial(
            [FromQuery] string? mes,
            [FromQuery] string? criterio)
        {
            var usuarioId = User.ObtenerUsuarioId();
            var resultado = await _potencialServicio.CalcularPotencialMensualAsync(usuarioId, mes, criterio);
            return Ok(resultado);
        }
    }
}