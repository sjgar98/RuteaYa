using System.ComponentModel.DataAnnotations;
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

        /// <summary>Registra un viaje habitual del usuario autenticado.</summary>
        /// <response code="201">Viaje creado.</response>
        /// <response code="400">Datos inválidos.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ViajeDto>> Crear([FromBody] CrearViajeDto dto)
        {
            // TODO Autenticación: tomar el usuario del token y quitar el valor fijo.
            var usuarioId = 1;

            var viaje = await _viajeServicio.CrearAsync(usuarioId, dto);

            // TODO Detalle del viaje: usar CreatedAtAction cuando exista GET api/viajes/{id}.
            return StatusCode(StatusCodes.Status201Created, viaje);
        }

        /// <summary>Busca direcciones que coincidan con el texto (origen, destino o paradas).</summary>
        /// <param name="texto">Texto a buscar, al menos 3 caracteres.</param>
        [HttpGet("direcciones")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<DireccionDto>>> BuscarDirecciones([FromQuery, Required, MinLength(3)] string texto)
        {
            var direcciones = await _viajeServicio.BuscarDireccionesAsync(texto);
            return Ok(direcciones);
        }
    }
}
