using Microsoft.AspNetCore.Mvc;

namespace RuteaYa.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
