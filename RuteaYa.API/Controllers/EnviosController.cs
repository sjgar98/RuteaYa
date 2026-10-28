using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Extensiones;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EnviosController : ControllerBase
    {
        private readonly IEnvioServicio _envioServicio;

        public EnviosController(IEnvioServicio envioServicio)
        {
            _envioServicio = envioServicio;
        }

        /// <summary>Lista los envíos del usuario autenticado.</summary>
        /// <param name="estado">Filtra por estado; sin valor devuelve todos.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<EnvioDto>>> Listar([FromQuery] EstadoEnvio? estado)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var envios = await _envioServicio.ListarPorUsuarioAsync(usuarioId, estado);
            return Ok(envios);
        }

        /// <summary>Obtiene el detalle de un envío del usuario autenticado, con el viaje asociado y el historial de estados.</summary>
        /// <response code="200">Detalle del envío.</response>
        /// <response code="404">El envío no existe o pertenece a otro usuario.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EnvioDetalleDto>> Obtener(int id)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var envio = await _envioServicio.ObtenerDetalleAsync(usuarioId, id);
            if (envio == null)
            {
                return NotFound();
            }

            return Ok(envio);
        }

        /// <summary>Cancela un envío del usuario autenticado (solo si no fue retirado ni está en un estado final).</summary>
        /// <response code="204">Envío cancelado.</response>
        /// <response code="404">El envío no existe o pertenece a otro usuario.</response>
        /// <response code="409">El envío no admite cancelación en su estado actual.</response>
        [HttpPost("{id:int}/cancelar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Cancelar(int id)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var resultado = await _envioServicio.CancelarAsync(usuarioId, id);

            return resultado switch
            {
                ResultadoOperacion.NoEncontrado => NotFound(),
                ResultadoOperacion.Conflicto => Problem(statusCode: StatusCodes.Status409Conflict,
                    title: "El envío no admite cancelación en su estado actual."),
                _ => NoContent()
            };
        }

        /// <summary>Publica un nuevo envío del usuario autenticado.</summary>
        /// <response code="201">Envío creado.</response>
        /// <response code="400">Datos inválidos.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EnvioDto>> Crear([FromBody] CrearEnvioDto dto)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var envio = await _envioServicio.CrearAsync(usuarioId, dto);

            return CreatedAtAction(nameof(Obtener), new { id = envio.Id }, envio);
        }
    }
}