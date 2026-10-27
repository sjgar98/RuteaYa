using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Extensiones;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/historial")]
    public class HistorialController : ControllerBase
    {
        private readonly IHistorialServicio _historialServicio;

        public HistorialController(IHistorialServicio historialServicio)
        {
            _historialServicio = historialServicio;
        }

     
        [HttpGet]
        [ProducesResponseType(typeof(HistorialDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<HistorialDto>> ObtenerHistorial(
      [FromQuery] DateTime? desde,
      [FromQuery] DateTime? hasta,
      [FromQuery] string? orden,
      [FromQuery] string? estado)
        {
            var usuarioId = User.ObtenerUsuarioId();
            var resultado = await _historialServicio.ObtenerHistorialAsync(usuarioId, desde, hasta, orden, estado);
            return Ok(resultado);
        }
    }
}