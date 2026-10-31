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
    public class OptimizacionController : ControllerBase
    {
        private readonly IOptimizadorServicio _optimizadorServicio;

        public OptimizacionController(IOptimizadorServicio optimizadorServicio)
        {
            _optimizadorServicio = optimizadorServicio;
        }

        /// <summary>
        /// Calcula la combinación de envíos que maximiza el criterio elegido para un viaje del usuario,
        /// respetando cantidad de paquetes, peso máximo y desvío máximo.
        /// También devuelve otras combinaciones válidas encontradas y los envíos descartados con su motivo.
        /// </summary>
        /// <param name="viajeId">ID del viaje a optimizar.</param>
        /// <param name="criterio">"ganancia" (default), "desvio" o "equilibrio". En empate de ganancia se prefiere el menor desvío.</param>
        /// <response code="200">Resultado del optimizador con la combinación óptima, otras combinaciones y descartados.</response>
        /// <response code="404">El viaje no existe o no pertenece al usuario autenticado.</response>
        [HttpGet("{viajeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OptimizacionResultadoDto>> Optimizar(
            int viajeId,
            [FromQuery] string? criterio = "ganancia")
        {
            var resultado = await _optimizadorServicio.OptimizarAsync(User.ObtenerUsuarioId(), viajeId, criterio);
            if (resultado == null)
            {
                return NotFound();
            }

            return Ok(resultado);
        }
    }
}

