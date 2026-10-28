using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class EnviosController : Controller
    {
        private readonly IEnvioServicio _envioServicio;

        public EnviosController(IEnvioServicio envioServicio)
        {
            _envioServicio = envioServicio;
        }
        public async Task<IActionResult> Index(EstadoEnvio? estado)
        {
            var modelo = new MisEnviosViewModel
            {
                Envios = await _envioServicio.ListarAsync(estado),
                Estado = estado
            };

            return View(modelo);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var envio = await _envioServicio.ObtenerDetalleAsync(id);
            if (envio == null)
            {
                return NotFound();
            }

            return View(envio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var resultado = await _envioServicio.CancelarAsync(id);
            if (resultado.NoEncontrado)
            {
                return NotFound();
            }

            if (resultado.Conflicto != null)
            {
                var envio = await _envioServicio.ObtenerDetalleAsync(id);
                if (envio == null)
                {
                    return NotFound();
                }

                ViewData["Error"] = resultado.Conflicto;
                return View(nameof(Detalle), envio);
            }

            TempData["MensajeExito"] = "El envío se canceló correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Nuevo()
        {
            return View(new CrearEnvioDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nuevo(CrearEnvioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var errores = await _envioServicio.CrearAsync(dto);
            if (errores.Count > 0)
            {
                AgregarErrores(errores);
                return View(dto);
            }

            TempData["MensajeExito"] = "El envío se publicó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        private void AgregarErrores(Dictionary<string, string[]> errores)
        {
            foreach (var (campo, mensajes) in errores)
            {
                foreach (var mensaje in mensajes)
                {
                    ModelState.AddModelError(campo, mensaje);
                }
            }
        }
    }
}