using System.ComponentModel.DataAnnotations;
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
            var usuarioId = User.ObtenerUsuarioId();

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
            var usuarioId = User.ObtenerUsuarioId();

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
            var usuarioId = User.ObtenerUsuarioId();

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
