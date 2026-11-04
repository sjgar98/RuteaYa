using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class PotencialController : Controller
    {
        private static readonly string[] _criterios = { "ganancia", "desvio", "equilibrio" };
        private static readonly TimeSpan _offsetArgentina = TimeSpan.FromHours(-3);

        private readonly IPotencialServicio _potencialServicio;

        public PotencialController(IPotencialServicio potencialServicio)
        {
            _potencialServicio = potencialServicio;
        }

        public async Task<IActionResult> Index(string? mes, string? criterio)
        {
            var criterioNorm = _criterios.Contains(criterio) ? criterio! : "ganancia";
            var mesNorm = string.IsNullOrWhiteSpace(mes) ? DateTime.UtcNow.Add(_offsetArgentina).ToString("yyyy-MM") : mes!;

            var resultado = await _potencialServicio.ObtenerPotencialAsync(mesNorm, criterioNorm);
            if (resultado == null)
            {
                ViewData["Error"] = "El mes elegido no es válido.";
                resultado = new PotencialMensualDto { Mes = mesNorm, Criterio = criterioNorm };
            }

            ViewData["MesActual"] = mesNorm;
            ViewData["CriterioActual"] = criterioNorm;

            return View(resultado);
        }
    }
}
