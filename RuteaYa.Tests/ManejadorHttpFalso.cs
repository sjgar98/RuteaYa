using System.Net;
using System.Text;

namespace RuteaYa.Tests
{
    // Handler que devuelve respuestas (o falla) en el orden en que se encolaron y registra las URLs pedidas.
    internal sealed class ManejadorHttpFalso : HttpMessageHandler
    {
        private readonly Queue<Func<HttpResponseMessage>> _respuestas = new Queue<Func<HttpResponseMessage>>();

        public int CantidadPeticiones { get; private set; }
        public List<string> Urls { get; } = new List<string>();
        public List<string> UserAgents { get; } = new List<string>();

        public void Encolar(HttpStatusCode estado, string json)
        {
            _respuestas.Enqueue(() => new HttpResponseMessage(estado)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        public void EncolarFallo(Exception fallo)
        {
            _respuestas.Enqueue(() => throw fallo);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
        {
            CantidadPeticiones++;
            Urls.Add(peticion.RequestUri!.AbsoluteUri);
            UserAgents.Add(peticion.Headers.UserAgent.ToString());

            if (_respuestas.Count == 0)
            {
                throw new InvalidOperationException("Llegó una petición que el test no esperaba.");
            }
            return Task.FromResult(_respuestas.Dequeue()());
        }
    }
}