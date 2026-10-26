using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Models;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [Authorize]
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
                AgregarErrores(errores);
                return View(dto);
            }

            // El layout lo muestra como toast.
            TempData["MensajeExito"] = "El viaje se guardó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var viaje = await _viajeServicio.ObtenerDetalleAsync(id);
            if (viaje == null)
            {
                return NotFound();
            }

            // En Detalle el botón Editar está deshabilitado; esto cubre el acceso por URL.
            if (viaje.TieneEnviosPendientes)
            {
                return RedirectToAction(nameof(Detalle), new { id });
            }

            ViewData["IdViaje"] = id;
            return View(AFormulario(viaje));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CrearViajeDto dto)
        {
            // _FormularioViaje lo usa para postear a Editar/{id} y volver al detalle.
            ViewData["IdViaje"] = id;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var resultado = await _viajeServicio.ActualizarAsync(id, dto);
            if (resultado.NoEncontrado)
            {
                return NotFound();
            }

            if (resultado.Conflicto != null)
            {
                ModelState.AddModelError(string.Empty, resultado.Conflicto);
                return View(dto);
            }

            if (resultado.Errores.Count > 0)
            {
                AgregarErrores(resultado.Errores);
                return View(dto);
            }

            TempData["MensajeExito"] = "El viaje se actualizó correctamente.";
            return RedirectToAction(nameof(Detalle), new { id });
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

        // Errores de validación de la API, para mostrarlos junto a cada campo.
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

        // Prepobla el formulario de edición con los datos actuales del viaje.
        private static CrearViajeDto AFormulario(ViajeDetalleDto viaje)
        {
            return new CrearViajeDto
            {
                OrigenDir = viaje.OrigenDir,
                OrigenLat = viaje.OrigenLat,
                OrigenLng = viaje.OrigenLng,
                DestinoDir = viaje.DestinoDir,
                DestinoLat = viaje.DestinoLat,
                DestinoLng = viaje.DestinoLng,
                Paradas = viaje.Paradas,
                Dias = viaje.Dias,
                HoraSalida = viaje.HoraSalida,
                HoraLlegada = viaje.HoraLlegada,
                MaxPaquetes = viaje.MaxPaquetes,
                PesoMaxKg = SinCerosFinales(viaje.PesoMaxKg),
                DesvioMaxKm = SinCerosFinales(viaje.DesvioMaxKm)
            };
        }

        // La BD guarda decimal(18,6): sin esto el input muestra "10.500000" en vez de "10.5".
        private static decimal SinCerosFinales(decimal valor)
        {
            return valor / 1.0000000000000000000000000000m;
        }
    }
}
