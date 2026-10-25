using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.DTOs;
using RuteaYa.MVC.Servicios.Interfaces;

namespace RuteaYa.MVC.Controllers
{
    [AllowAnonymous]
    public class CuentaController : Controller
    {
        private readonly ICuentaServicio _cuentaServicio;

        public CuentaController(ICuentaServicio cuentaServicio)
        {
            _cuentaServicio = cuentaServicio;
        }

        // returnUrl: página a la que quiso entrar sin sesión (la agrega la cookie al redirigir acá).
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var sesion = await _cuentaServicio.LoginAsync(dto);
            if (sesion == null)
            {
                // Mensaje genérico: no se indica si falló el email o la contraseña.
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return View(dto);
            }

            await IniciarSesionAsync(sesion);

            // Solo URLs locales, para que no se pueda redirigir a otro sitio después del login.
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegistroUsuarioDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (sesion, errores) = await _cuentaServicio.RegistrarAsync(dto);
            if (sesion == null)
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

            // Al registrarse queda logueado.
            await IniciarSesionAsync(sesion);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // Guarda en la cookie los datos del usuario y el JWT de la API (claim "token", lo usa TokenHandler).
        private async Task IniciarSesionAsync(SesionDto sesion)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, sesion.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, $"{sesion.Nombre} {sesion.Apellido}"),
                new Claim(ClaimTypes.GivenName, sesion.Nombre),
                new Claim(ClaimTypes.Email, sesion.Email),
                new Claim("token", sesion.Token)
            };

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // La cookie vence junto con el token: si durara más, la API respondería 401 con la sesión abierta.
            var propiedades = new AuthenticationProperties { ExpiresUtc = sesion.ExpiraEn };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad), propiedades);
        }
    }
}
