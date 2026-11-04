namespace RuteaYa.MVC.Models
{
    public class ResultadoConfirmacion
    {
        public bool NoEncontrado { get; set; }

        // Motivo que devolvió la API (409); null si se reservaron todos.
        public string? Conflicto { get; set; }

        public int Cantidad { get; set; }
    }
}
