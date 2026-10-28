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
    public class TransportesController : ControllerBase
    {
        private readonly ITransporteServicio _transporteServicio;

        public TransportesController(ITransporteServicio transporteServicio)
        {
            _transporteServicio = transporteServicio;
        }

        /// <summary>Lista los paquetes que el usuario autenticado aceptó transportar.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<TransporteDto>>> Listar()
        {
            var usuarioId = User.ObtenerUsuarioId();

            var transportes = await _transporteServicio.ListarAsync(usuarioId);
            return Ok(transportes);
        }

        /// <summary>Marca un paquete como retirado.</summary>
        /// <response code="204">Estado actualizado.</response>
        /// <response code="404">La asignación no existe o pertenece a otro usuario.</response>
        /// <response code="409">La transición de estado no es válida.</response>
        [HttpPost("{asignacionId:int}/retirado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> MarcarRetirado(int asignacionId)
        {
            return CambiarEstadoAsync(() => _transporteServicio.MarcarRetiradoAsync(User.ObtenerUsuarioId(), asignacionId));
        }

        /// <summary>Marca un paquete como en camino.</summary>
        /// <response code="204">Estado actualizado.</response>
        /// <response code="404">La asignación no existe o pertenece a otro usuario.</response>
        /// <response code="409">La transición de estado no es válida.</response>
        [HttpPost("{asignacionId:int}/en-camino")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> MarcarEnCamino(int asignacionId)
        {
            return CambiarEstadoAsync(() => _transporteServicio.MarcarEnCaminoAsync(User.ObtenerUsuarioId(), asignacionId));
        }

        /// <summary>Marca un paquete como entregado.</summary>
        /// <response code="204">Estado actualizado.</response>
        /// <response code="404">La asignación no existe o pertenece a otro usuario.</response>
        /// <response code="409">La transición de estado no es válida.</response>
        [HttpPost("{asignacionId:int}/entregado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> MarcarEntregado(int asignacionId)
        {
            return CambiarEstadoAsync(() => _transporteServicio.MarcarEntregadoAsync(User.ObtenerUsuarioId(), asignacionId));
        }

        private static async Task<IActionResult> CambiarEstadoAsync(Func<Task<ResultadoOperacion>> accion)
        {
            var resultado = await accion();

            return resultado switch
            {
                ResultadoOperacion.NoEncontrado => new NotFoundResult(),
                ResultadoOperacion.Conflicto => new ObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "La transición de estado no es válida."
                })
                { StatusCode = StatusCodes.Status409Conflict },
                _ => new NoContentResult()
            };
        }
    }
}