using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.Entidades;

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
            // TODO Autenticación: tomar el usuario del token y quitar el usuario de prueba
            // (requiere using System.Security.Claims).
            // var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuarioId = UsuarioPrueba.Id;

            var viajes = await _viajeServicio.ListarPorUsuarioAsync(usuarioId, activo);
            return Ok(viajes);
        }

        /// <summary>Obtiene el detalle de un viaje del usuario autenticado, con sus paradas y paquetes asignados.</summary>
        /// <response code="200">Detalle del viaje.</response>
        /// <response code="404">El viaje no existe o pertenece a otro usuario.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ViajeDetalleDto>> Obtener(int id)
        {
            // TODO Autenticación: tomar el usuario del token y quitar el usuario de prueba
            // (requiere using System.Security.Claims).
            // var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuarioId = UsuarioPrueba.Id;

            var viaje = await _viajeServicio.ObtenerDetalleAsync(usuarioId, id);
            if (viaje == null)
            {
                return NotFound();
            }

            return Ok(viaje);
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
            // TODO Autenticación: tomar el usuario del token y quitar el usuario de prueba
            // (requiere using System.Security.Claims).
            // var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuarioId = UsuarioPrueba.Id;

            var viaje = await _viajeServicio.CrearAsync(usuarioId, dto);

            return CreatedAtAction(nameof(Obtener), new { id = viaje.Id }, viaje);
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
