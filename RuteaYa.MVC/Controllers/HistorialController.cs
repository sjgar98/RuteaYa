using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class HistorialController : Controller
    {
        private readonly IHistorialServicio _historialServicio;

        public HistorialController(IHistorialServicio historialServicio)
        {
            _historialServicio = historialServicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string? orden, string? estado)
        {
            var modelo = await _historialServicio.ObtenerHistorialAsync(desde, hasta, orden, estado);
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Orden = orden;
            ViewBag.Estado = estado;
            return View(modelo ?? new RuteaYa.DTOs.HistorialDto());
        }
    }
}