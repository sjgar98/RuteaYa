using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class PotencialController : Controller
    {
        private readonly IPotencialServicio _potencialServicio;

        public PotencialController(IPotencialServicio potencialServicio)
        {
            _potencialServicio = potencialServicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? mes, string? criterio)
        {
            // El MVC llama al servicio HTTP que consulta a la API
            var modelo = await _potencialServicio.ObtenerPotencialAsync(mes, criterio);

            ViewBag.MesSeleccionado = string.IsNullOrEmpty(mes) ? DateTime.Now.ToString("yyyy-MM") : mes;
            ViewBag.CriterioSeleccionado = string.IsNullOrEmpty(criterio) ? "ganancia" : criterio;

            return View(modelo ?? new RuteaYa.DTOs.PotencialDto());
        }
    }
}