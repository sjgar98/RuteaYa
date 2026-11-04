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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PotencialMensualDto>> GetPotencial(
            [FromQuery] string mes,
            [FromQuery] string? criterio = "ganancia")
        {
            var usuarioId = User.ObtenerUsuarioId();
            var resultado = await _potencialServicio.ObtenerPotencialAsync(usuarioId, mes, criterio);

            if (resultado == null)
            {
                return NotFound();
            }

            return Ok(resultado);
        }
    }
}