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
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardServicio _dashboardServicio;

        public DashboardController(IDashboardServicio dashboardServicio)
        {
            _dashboardServicio = dashboardServicio;
        }

        /// <summary>
        /// Resumen del usuario autenticado: próximas salidas de sus viajes, envíos pendientes,
        /// cantidad de oportunidades y ganancia potencial del mes (estimada).
        /// </summary>
        /// <response code="200">Resumen del usuario.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DashboardDto>> Obtener()
        {
            var dashboard = await _dashboardServicio.ObtenerAsync(User.ObtenerUsuarioId());
            return Ok(dashboard);
        }
    }
}
