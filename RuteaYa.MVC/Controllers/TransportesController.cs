using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class TransportesController : Controller
    {
        private readonly ITransporteServicio _transporteServicio;

        public TransportesController(ITransporteServicio transporteServicio)
        {
            _transporteServicio = transporteServicio;
        }

        public async Task<IActionResult> Index()
        {
            var modelo = new MisTransportesViewModel
            {
                Transportes = await _transporteServicio.ListarAsync()
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> MarcarRetirado(int asignacionId)
        {
            return AplicarCambioAsync(() => _transporteServicio.MarcarRetiradoAsync(asignacionId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> MarcarEnCamino(int asignacionId)
        {
            return AplicarCambioAsync(() => _transporteServicio.MarcarEnCaminoAsync(asignacionId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> MarcarEntregado(int asignacionId)
        {
            return AplicarCambioAsync(() => _transporteServicio.MarcarEntregadoAsync(asignacionId));
        }

        private async Task<IActionResult> AplicarCambioAsync(Func<Task<ResultadoCambioEstado>> accion)
        {
            var resultado = await accion();
            if (resultado.NoEncontrado)
            {
                return NotFound();
            }

            if (resultado.Conflicto != null)
            {
                var modelo = new MisTransportesViewModel
                {
                    Transportes = await _transporteServicio.ListarAsync(),
                    Error = resultado.Conflicto
                };
                return View(nameof(Index), modelo);
            }

            TempData["MensajeExito"] = "El estado se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}