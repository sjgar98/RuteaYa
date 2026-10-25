using System.Security.Claims;

namespace RuteaYa.API.Extensiones
{
    public static class ClaimsPrincipalExtensiones
    {
        // Id del usuario autenticado, tomado del JWT. Usar solo en acciones con [Authorize].
        public static int ObtenerUsuarioId(this ClaimsPrincipal usuario)
        {
            return int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
