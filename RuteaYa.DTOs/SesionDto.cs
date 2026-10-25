namespace RuteaYa.DTOs
{
    // Respuesta del login y del registro: el JWT para llamar a la API y los datos para el menú del usuario.
    public class SesionDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
