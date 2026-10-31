using System.ComponentModel.DataAnnotations;
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

        /// <summary>Modifica un viaje del usuario autenticado (mismos datos y validaciones que al crearlo; las paradas se reemplazan).</summary>
        /// <response code="204">Viaje actualizado.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="404">El viaje no existe o pertenece a otro usuario.</response>
        /// <response code="409">El viaje tiene envíos pendientes (asignados, retirados o en camino).</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] CrearViajeDto dto)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var resultado = await _viajeServicio.ActualizarAsync(usuarioId, id, dto);

            return resultado switch
            {
                ResultadoOperacion.NoEncontrado => NotFound(),
                ResultadoOperacion.Conflicto => Problem(statusCode: StatusCodes.Status409Conflict,
                    title: "El viaje tiene envíos pendientes y no se puede editar."),
                _ => NoContent()
            };
        }

        /// <summary>Desactiva un viaje del usuario autenticado (deja de recibir oportunidades; no se borra).</summary>
        /// <response code="204">Viaje desactivado.</response>
        /// <response code="404">El viaje no existe o pertenece a otro usuario.</response>
        /// <response code="409">El viaje tiene envíos pendientes (asignados, retirados o en camino).</response>
        [HttpPost("{id:int}/desactivar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var resultado = await _viajeServicio.CambiarActivoAsync(usuarioId, id, false);

            return resultado switch
            {
                ResultadoOperacion.NoEncontrado => NotFound(),
                ResultadoOperacion.Conflicto => Problem(statusCode: StatusCodes.Status409Conflict,
                    title: "El viaje tiene envíos pendientes y no se puede desactivar."),
                _ => NoContent()
            };
        }

        /// <summary>Vuelve a activar un viaje del usuario autenticado.</summary>
        /// <response code="204">Viaje activado.</response>
        /// <response code="404">El viaje no existe o pertenece a otro usuario.</response>
        [HttpPost("{id:int}/activar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Activar(int id)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var resultado = await _viajeServicio.CambiarActivoAsync(usuarioId, id, true);

            return resultado == ResultadoOperacion.NoEncontrado ? NotFound() : NoContent();
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
