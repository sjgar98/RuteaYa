using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface IUsuarioServicio
    {
        // null si ya existe un usuario con ese email.
        Task<SesionDto?> RegistrarAsync(RegistroUsuarioDto dto);

        // null si el email no existe o la contraseña no coincide (el controller no distingue cuál falló).
        Task<SesionDto?> LoginAsync(LoginDto dto);
    }
}
