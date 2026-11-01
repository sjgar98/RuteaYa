using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDashboardServicio _dashboardServicio;

        public HomeController(IDashboardServicio dashboardServicio)
        {
            _dashboardServicio = dashboardServicio;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dashboardServicio.ObtenerAsync());
        }
    }
}
