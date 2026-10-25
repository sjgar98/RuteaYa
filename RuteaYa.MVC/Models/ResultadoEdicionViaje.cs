namespace RuteaYa.MVC.Models
{
    // Respuesta de la API al editar un viaje, para que el controller arme la vista.
    public class ResultadoEdicionViaje
    {
        // 404: no existe o es de otro usuario.
        public bool NoEncontrado { get; set; }

        // 409: mensaje del conflicto (p. ej. envíos pendientes).
        public string? Conflicto { get; set; }

        // 400: errores de validación por campo.
        public Dictionary<string, string[]> Errores { get; set; } = new Dictionary<string, string[]>();
    }
}
