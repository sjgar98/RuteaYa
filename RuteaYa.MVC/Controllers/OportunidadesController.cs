using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class OportunidadesController : Controller
    {
        private readonly IOportunidadesServicio _oportunidadesServicio;

        public OportunidadesController(IOportunidadesServicio oportunidadesServicio)
        {
            _oportunidadesServicio = oportunidadesServicio;
        }

        /// <summary>
        /// Pantalla 8: Listado de oportunidades (envíos compatibles) para un viaje.
        /// </summary>
        public async Task<IActionResult> Index(int viajeId, string? ordenarPor = "ganancia")
        {
            ViewData["ViajeId"] = viajeId;
            ViewData["OrdenarPor"] = ordenarPor;

            var oportunidades = await _oportunidadesServicio.ObtenerOportunidadesAsync(viajeId, ordenarPor);

            return View(oportunidades);
        }

        /// <summary>
        /// Pantalla 9: Detalle de una oportunidad específica.
        /// </summary>
        public async Task<IActionResult> Detalle(int viajeId, int envioId)
        {
            var detalle = await _oportunidadesServicio.ObtenerDetalleAsync(viajeId, envioId);
            if (detalle == null)
            {
                return NotFound();
            }

            ViewData["ViajeId"] = viajeId;

            return View(detalle);
        }

        /// <summary>
        /// Acepta una oportunidad (POST desde el detalle).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(int viajeId, int envioId)
        {
            var resultado = await _oportunidadesServicio.AceptarAsync(viajeId, envioId);

            if (resultado.Exitoso)
            {
                // Redirigir a listado con mensaje de éxito
                TempData["Mensaje"] = "¡Oportunidad aceptada correctamente!";
                TempData["Tipo"] = "success";
                return RedirectToAction("Index", new { viajeId });
            }

            if (resultado.NoEncontrado)
            {
                return NotFound();
            }

            // Redirigir al detalle con mensaje de error
            TempData["Mensaje"] = resultado.Conflicto ?? "No se pudo aceptar la oportunidad.";
            TempData["Tipo"] = "error";
            return RedirectToAction("Detalle", new { viajeId, envioId });
        }
    }
}
