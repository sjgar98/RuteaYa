using Microsoft.AspNetCore.Mvc;
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
    }
}
