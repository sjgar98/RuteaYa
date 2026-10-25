using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Controllers
{
    // TODO Autenticación: descomentar cuando esté configurado JWT en Program.cs.
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ViajesController : ControllerBase
    {
        private readonly IViajeServicio _viajeServicio;

        public ViajesController(IViajeServicio viajeServicio)
        {
            _viajeServicio = viajeServicio;
        }

        /// <summary>Lista los viajes del usuario autenticado.</summary>
        /// <param name="activo">true: activos, false: inactivos, sin valor: todos.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ViajeDto>>> Listar([FromQuery] bool? activo)
        {
            // TODO Autenticación: tomar el usuario del token y quitar el valor fijo.
            // var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuarioId = 1;

            var viajes = await _viajeServicio.ListarPorUsuarioAsync(usuarioId, activo);
            return Ok(viajes);
        }
    }
}
