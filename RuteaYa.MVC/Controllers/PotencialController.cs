using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
    public class PotencialController : Controller
    {
        private static readonly string[] _criterios = { "todos", "ganancia", "desvio", "equilibrio" };

        private readonly IPotencialServicio _potencialServicio;

        public PotencialController(IPotencialServicio potencialServicio)
        {
            _potencialServicio = potencialServicio;
        }

        // Pantalla 14. Muestra la estimación del potencial mensual según mes y criterio de optimización.
        public async Task<IActionResult> Index(string? mes, string? criterio)
        {
            // Normalizamos el criterio de optimización por seguridad
            var criterioNorm = _criterios.Contains(criterio) ? criterio! : "ganancia";

            // Si no viene mes por parámetro, tomamos el mes actual (yyyy-MM)
            var mesNorm = string.IsNullOrWhiteSpace(mes) ? DateTime.UtcNow.ToString("yyyy-MM") : mes!;

            var resultado = await _potencialServicio.ObtenerPotencialAsync(mesNorm, criterioNorm);

            if (resultado == null)
            {
                // Si la API no retorna datos, inicializamos un DTO vacío para que la vista renderice el estado sin romper
                resultado = new PotencialMensualDto
                {
                    MejorRecorrido = new List<DetalleViajePotencialDto>()
                };
            }

            // Sincronizamos los ViewData para que los inputs y botones de la vista conserven su estado visual
            ViewData["MesActual"] = mesNorm;
            ViewData["CriterioActual"] = criterioNorm;

            return View(resultado);
        }
    }
}