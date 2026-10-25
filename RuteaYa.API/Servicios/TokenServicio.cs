using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.Entidades;

namespace RuteaYa.API.Servicios
{
    public class TokenServicio : ITokenServicio
    {
        private readonly IConfiguration _configuracion;

        public TokenServicio(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public SesionDto CrearSesion(Usuario usuario)
        {
            var expiraEn = DateTime.UtcNow.AddMinutes(_configuracion.GetValue("Jwt:ExpiracionMinutos", 480));
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracion["Jwt:Key"]!));

            // Los controllers leen el id con User.ObtenerUsuarioId() (claim NameIdentifier).
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                    new Claim(ClaimTypes.Email, usuario.Email)
                }),
                Expires = expiraEn,
                SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
            };

            return new SesionDto
            {
                Token = new JsonWebTokenHandler().CreateToken(descriptor),
                ExpiraEn = expiraEn,
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email
            };
        }
    }
}
