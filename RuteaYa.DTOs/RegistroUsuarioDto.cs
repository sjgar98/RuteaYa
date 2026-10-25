using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs
{
    public class RegistroUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
        [StringLength(256, ErrorMessage = "El email no puede superar los 256 caracteres.")]
        public string Email { get; set; } = string.Empty;

        // Requisitos definidos por el grupo: al menos 8 caracteres, con una letra y un número.
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contraseña debe tener al menos una letra y un número.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmá la contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
