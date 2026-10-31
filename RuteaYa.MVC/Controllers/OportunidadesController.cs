using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class OportunidadesController : Controller
    {
        private static readonly string[] _ordenes = { "ganancia", "desvio", "compatibilidad" };

        private readonly IOportunidadesServicio _oportunidadesServicio;
        private readonly IViajeServicio _viajeServicio;

        public OportunidadesController(IOportunidadesServicio oportunidadesServicio, IViajeServicio viajeServicio)
        {
            _oportunidadesServicio = oportunidadesServicio;
            _viajeServicio = viajeServicio;
        }

        // Pantalla 8. Sin viajeId (desde el menú) muestra el primer viaje activo.
        public async Task<IActionResult> Index(int? viajeId, string? ordenarPor)
        {
            var modelo = new OportunidadesViewModel
            {
                Viajes = await _viajeServicio.ListarAsync(true),
                OrdenarPor = _ordenes.Contains(ordenarPor) ? ordenarPor! : "ganancia"
            };

            viajeId ??= modelo.Viajes.FirstOrDefault()?.Id;
            if (viajeId.HasValue)
            {
                modelo.Viaje = await _oportunidadesServicio.ListarAsync(viajeId.Value, modelo.OrdenarPor);
                if (modelo.Viaje == null)
                {
                    return NotFound();
                }
            }

            ViewData["Error"] = TempData["Error"];
            return View(modelo);
        }

        // Pantalla 9.
        public async Task<IActionResult> Detalle(int viajeId, int envioId)
        {
            var detalle = await _oportunidadesServicio.ObtenerDetalleAsync(viajeId, envioId);
            if (detalle == null)
            {
                TempData["Error"] = "La oportunidad ya no está disponible.";
                return RedirectToAction(nameof(Index), new { viajeId });
            }

            ViewData["Error"] = TempData["Error"];
            return View(detalle);
        }

        // Se puede aceptar desde el listado o desde el detalle; ante un conflicto se vuelve a donde estaba.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(int viajeId, int envioId, bool desdeDetalle = false)
        {
            var resultado = await _oportunidadesServicio.AceptarAsync(viajeId, envioId);
            if (resultado.NoEncontrado)
            {
                return NotFound();
            }

            if (resultado.Conflicto != null)
            {
                TempData["Error"] = resultado.Conflicto;
                return desdeDetalle
                    ? RedirectToAction(nameof(Detalle), new { viajeId, envioId })
                    : RedirectToAction(nameof(Index), new { viajeId });
            }

            TempData["MensajeExito"] = "Aceptaste el envío. Lo vas a ver en Mis transportes.";
            return RedirectToAction(nameof(Index), new { viajeId });
        }
    }
}
