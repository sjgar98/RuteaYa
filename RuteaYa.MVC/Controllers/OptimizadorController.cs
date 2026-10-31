using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class OptimizadorController : Controller
    {
        private static readonly string[] _criterios = { "ganancia", "desvio", "equilibrio" };

        private readonly IOptimizadorServicio _optimizadorServicio;
        private readonly IViajeServicio _viajeServicio;

        public OptimizadorController(IOptimizadorServicio optimizadorServicio, IViajeServicio viajeServicio)
        {
            _optimizadorServicio = optimizadorServicio;
            _viajeServicio = viajeServicio;
        }

        // Pantalla 10. Muestra la combinación óptima, otras combinaciones y descartados.
        public async Task<IActionResult> Index(int? viajeId, string? criterio)
        {
            var criterioNorm = _criterios.Contains(criterio) ? criterio! : "ganancia";

            // Sin viajeId (desde el menú) toma el primer viaje activo del usuario.
            if (!viajeId.HasValue)
            {
                var viajes = await _viajeServicio.ListarAsync(true);
                viajeId = viajes.FirstOrDefault()?.Id;
            }

            if (!viajeId.HasValue)
            {
                // El usuario no tiene viajes activos: mostrar estado vacío.
                return View(null);
            }

            var resultado = await _optimizadorServicio.OptimizarAsync(viajeId.Value, criterioNorm);
            if (resultado == null)
            {
                return NotFound();
            }

            ViewData["Criterio"] = criterioNorm;
            ViewData["ViajeId"] = viajeId.Value;
            return View(resultado);
        }

        // POST: confirmar la combinación óptima y crear asignaciones vía la API de Oportunidades.
        // Cada envío se acepta individualmente usando el endpoint existente de P5.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int viajeId, [FromForm] List<int> envioIds)
        {
            if (!envioIds.Any())
            {
                TempData["Error"] = "No seleccionaste ningún envío para confirmar.";
                return RedirectToAction(nameof(Index), new { viajeId });
            }

            var conflictos = new List<string>();
            var aceptados = 0;

            // Acepta cada envío usando el servicio de Oportunidades del MVC (ya existe en IOportunidadesServicio).
            // El 409 de la API indica que alguien se adelantó: se informa el conflicto.
            var oportunidadesServicio = HttpContext.RequestServices.GetRequiredService<IOportunidadesServicio>();
            foreach (var envioId in envioIds)
            {
                var resultado = await oportunidadesServicio.AceptarAsync(viajeId, envioId);
                if (resultado.Conflicto != null)
                {
                    conflictos.Add(resultado.Conflicto);
                }
                else if (!resultado.NoEncontrado)
                {
                    aceptados++;
                }
            }

            if (conflictos.Any())
            {
                // Hubo conflictos: informar y ofrecer recalcular.
                TempData["Error"] = $"Se aceptaron {aceptados} envíos, pero hubo conflictos: {string.Join("; ", conflictos)} Podés recalcular la combinación.";
                return RedirectToAction(nameof(Index), new { viajeId });
            }

            TempData["MensajeExito"] = $"¡Combinación confirmada! Se aceptaron {aceptados} envíos. Los vas a ver en Mis transportes.";
            return RedirectToAction("Index", "Transportes");
        }
    }
}

