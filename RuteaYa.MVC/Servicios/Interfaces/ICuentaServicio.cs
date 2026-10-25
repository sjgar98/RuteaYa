using RuteaYa.DTOs;

namespace RuteaYa.MVC.Servicios.Interfaces
{
    public interface ICuentaServicio
    {
        // null si las credenciales son incorrectas (401 de la API).
        Task<SesionDto?> LoginAsync(LoginDto dto);

        // Sesion si se creó la cuenta; si no, los errores por campo (validación o email repetido).
        Task<(SesionDto? Sesion, Dictionary<string, string[]> Errores)> RegistrarAsync(RegistroUsuarioDto dto);
    }
}
