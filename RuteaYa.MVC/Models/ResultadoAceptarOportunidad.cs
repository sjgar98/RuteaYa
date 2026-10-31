namespace RuteaYa.MVC.Models
{
    public class ResultadoAceptarOportunidad
    {
        public bool NoEncontrado { get; set; }

        // Motivo que devolvió la API (409); null si se aceptó.
        public string? Conflicto { get; set; }
    }
}
