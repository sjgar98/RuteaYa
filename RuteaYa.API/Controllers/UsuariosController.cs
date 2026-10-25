using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioServicio _usuarioServicio;

        public UsuariosController(IUsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        /// <summary>Registra un usuario nuevo y devuelve su sesión (JWT), así queda logueado.</summary>
        /// <response code="201">Usuario creado.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="409">Ya existe una cuenta con ese email.</response>
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<SesionDto>> Registrar([FromBody] RegistroUsuarioDto dto)
        {
            var sesion = await _usuarioServicio.RegistrarAsync(dto);
            if (sesion == null)
            {
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Ya existe una cuenta con ese email.");
            }

            return StatusCode(StatusCodes.Status201Created, sesion);
        }

        /// <summary>Inicia sesión con email y contraseña y devuelve el JWT para llamar al resto de la API.</summary>
        /// <response code="200">Credenciales correctas.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="401">Email o contraseña incorrectos (no se indica cuál de los dos).</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SesionDto>> Login([FromBody] LoginDto dto)
        {
            var sesion = await _usuarioServicio.LoginAsync(dto);
            if (sesion == null)
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Email o contraseña incorrectos.");
            }

            return Ok(sesion);
        }
    }
}
