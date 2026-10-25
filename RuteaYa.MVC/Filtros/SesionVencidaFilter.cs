using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RuteaYa.MVC.Filtros
{
    // Si la API responde 401 (token vencido o inválido), los servicios lanzan HttpRequestException
    // desde EnsureSuccessStatusCode o GetFromJsonAsync. En vez de mostrar la excepción, se cierra la
    // sesión del MVC y se vuelve al login, que después redirige a la página que se estaba usando.
    public class SesionVencidaFilter : IAsyncExceptionFilter
    {
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is not HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
            {
                return;
            }

            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var request = context.HttpContext.Request;
            context.Result = new RedirectToActionResult("Login", "Cuenta", new { returnUrl = request.Path + request.QueryString });
            context.ExceptionHandled = true;
        }
    }
}
