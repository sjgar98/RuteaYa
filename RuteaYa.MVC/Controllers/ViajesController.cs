using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    // TODO Autenticación: descomentar cuando esté configurada la cookie en Program.cs.
    // [Authorize]
    public class ViajesController : Controller
    {
        private readonly IViajeServicio _viajeServicio;

        public ViajesController(IViajeServicio viajeServicio)
        {
            _viajeServicio = viajeServicio;
        }

        // Mis viajes. /Viajes muestra todos; /Viajes?activo=true o false filtra.
        public async Task<IActionResult> Index(bool? activo)
        {
            var modelo = new MisViajesViewModel
            {
                Viajes = await _viajeServicio.ListarAsync(activo),
                Activo = activo
            };

            return View(modelo);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var viaje = await _viajeServicio.ObtenerDetalleAsync(id);
            if (viaje == null)
            {
                return NotFound();
            }

            return View(viaje);
        }

        public IActionResult Nuevo()
        {
            return View(new CrearViajeDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nuevo(CrearViajeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var errores = await _viajeServicio.CrearAsync(dto);
            if (errores.Count > 0)
            {
                foreach (var (campo, mensajes) in errores)
                {
                    foreach (var mensaje in mensajes)
                    {
                        ModelState.AddModelError(campo, mensaje);
                    }
                }

                return View(dto);
            }

            // Mis viajes lo muestra como toast.
            TempData["MensajeExito"] = "El viaje se guardó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Autocompletado de origen, destino y paradas (lo llama la vista Nuevo con fetch).
        public async Task<IActionResult> BuscarDirecciones(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < 3)
            {
                return Json(new List<DireccionDto>());
            }

            return Json(await _viajeServicio.BuscarDireccionesAsync(texto.Trim()));
        }
    }
}
