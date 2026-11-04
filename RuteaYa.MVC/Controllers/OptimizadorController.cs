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
        private readonly IAsignacionServicio _asignacionServicio;

        public OptimizadorController(IOptimizadorServicio optimizadorServicio, IViajeServicio viajeServicio, IAsignacionServicio asignacionServicio)
        {
            _optimizadorServicio = optimizadorServicio;
            _viajeServicio = viajeServicio;
            _asignacionServicio = asignacionServicio;
        }

        // Pantalla 10. Muestra la combinación óptima, otras combinaciones y descartados.
        public async Task<IActionResult> Index(int? viajeId, string? criterio)
        {
            var criterioNorm = NormalizarCriterio(criterio);

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

        // Pantalla 11. Resumen de la selección antes de reservarla; no modifica nada.
        [HttpGet]
        public async Task<IActionResult> Confirmacion(int viajeId, [FromQuery] List<int> envioIds, string? criterio)
        {
            var criterioNorm = NormalizarCriterio(criterio);
            if (envioIds.Count == 0)
            {
                TempData["Error"] = "No seleccionaste ningún envío para confirmar.";
                return RedirectToAction(nameof(Index), new { viajeId, criterio = criterioNorm });
            }

            return await MostrarConfirmacionAsync(viajeId, envioIds, criterioNorm, conflicto: null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int viajeId, [FromForm] List<int> envioIds, string? criterio)
        {
            var criterioNorm = NormalizarCriterio(criterio);
            if (envioIds.Count == 0)
            {
                TempData["Error"] = "No seleccionaste ningún envío para confirmar.";
                return RedirectToAction(nameof(Index), new { viajeId, criterio = criterioNorm });
            }

            var resultado = await _asignacionServicio.ConfirmarAsync(viajeId, envioIds);
            if (resultado.NoEncontrado)
            {
                TempData["Error"] = "Alguno de los envíos ya no existe. Recalculá la combinación.";
                return RedirectToAction(nameof(Index), new { viajeId, criterio = criterioNorm });
            }

            if (resultado.Conflicto != null)
            {
                // Otro conductor se adelantó o cambió alguna restricción: se muestra el resumen actualizado.
                return await MostrarConfirmacionAsync(viajeId, envioIds, criterioNorm, resultado.Conflicto);
            }

            TempData["MensajeExito"] = resultado.Cantidad == 1
                ? "¡Listo! Reservaste 1 envío. Lo vas a ver en Mis transportes."
                : $"¡Listo! Reservaste {resultado.Cantidad} envíos. Los vas a ver en Mis transportes.";
            return RedirectToAction("Index", "Transportes");
        }

        private async Task<IActionResult> MostrarConfirmacionAsync(int viajeId, List<int> envioIds, string criterio, string? conflicto)
        {
            var resumen = await _asignacionServicio.ObtenerResumenAsync(viajeId, envioIds);
            if (resumen == null)
            {
                TempData["Error"] = "Alguno de los envíos ya no existe. Recalculá la combinación.";
                return RedirectToAction(nameof(Index), new { viajeId, criterio });
            }

            ViewData["Criterio"] = criterio;
            ViewData["Conflicto"] = conflicto;
            return View("Confirmacion", resumen);
        }

        private static string NormalizarCriterio(string? criterio)
        {
            return _criterios.Contains(criterio) ? criterio! : "ganancia";
        }
    }
}
