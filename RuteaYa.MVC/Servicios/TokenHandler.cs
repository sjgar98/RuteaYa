using System.Net.Http.Headers;

namespace RuteaYa.MVC.Servicios
{
    // Agrega el JWT del usuario logueado a cada llamada a la API ("Authorization: Bearer ...").
    // El login guarda el token emitido por la API como claim "token" en la cookie de autenticación.
    // Mientras no haya login, no hay claim y la llamada sale sin header.
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("token")?.Value;
            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
