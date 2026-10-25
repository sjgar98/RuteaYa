using RuteaYa.DTOs;
using RuteaYa.Entidades;

namespace RuteaYa.API.Servicios.Interfaces
{
    public interface ITokenServicio
    {
        // Emite el JWT del usuario y arma la sesión que devuelven el login y el registro.
        SesionDto CrearSesion(Usuario usuario);
    }
}
