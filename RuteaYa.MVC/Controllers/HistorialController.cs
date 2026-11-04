using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class HistorialController : Controller
    {
        private static readonly string[] _estados = { "todos", "viajes", "entregados", "cancelados" };
        private static readonly string[] _ordenes = { "fecha_desc", "fecha_asc", "monto_desc", "monto_asc" };

        private readonly IHistorialServicio _historialServicio;

        public HistorialController(IHistorialServicio historialServicio)
        {
            _historialServicio = historialServicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateOnly? desde, DateOnly? hasta, string? orden, string? estado)
        {
            var estadoNorm = _estados.Contains(estado) ? estado! : "todos";
            var ordenNorm = _ordenes.Contains(orden) ? orden! : "fecha_desc";

            var modelo = await _historialServicio.ObtenerHistorialAsync(desde, hasta, ordenNorm, estadoNorm);
            if (modelo == null)
            {
                ViewData["Error"] = "La fecha desde no puede ser posterior a la fecha hasta.";
            }

            ViewData["Desde"] = desde?.ToString("yyyy-MM-dd");
            ViewData["Hasta"] = hasta?.ToString("yyyy-MM-dd");
            ViewData["Orden"] = ordenNorm;
            ViewData["Estado"] = estadoNorm;
            return View(modelo ?? new HistorialDto());
        }
    }
}
