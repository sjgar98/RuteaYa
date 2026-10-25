using Microsoft.AspNetCore.Identity;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class UsuarioServicio : IUsuarioServicio
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly ITokenServicio _tokenServicio;

        public UsuarioServicio(IUsuarioRepositorio repositorio, IPasswordHasher<Usuario> passwordHasher, ITokenServicio tokenServicio)
        {
            _repositorio = repositorio;
            _passwordHasher = passwordHasher;
            _tokenServicio = tokenServicio;
        }

        // Las validaciones de los datos están en las Data Annotations de RegistroUsuarioDto.
        public async Task<SesionDto?> RegistrarAsync(RegistroUsuarioDto dto)
        {
            var email = NormalizarEmail(dto.Email);
            if (await _repositorio.ExisteEmailAsync(email))
            {
                return null;
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Email = email,
                CreadoEn = DateTime.UtcNow
            };
            // PasswordHasher genera un salt aleatorio y aplica PBKDF2: nunca se guarda la contraseña.
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, dto.Password);

            await _repositorio.AgregarAsync(usuario);

            return _tokenServicio.CrearSesion(usuario);
        }

        public async Task<SesionDto?> LoginAsync(LoginDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(NormalizarEmail(dto.Email));
            if (usuario == null)
            {
                return null;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);
            if (resultado == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return _tokenServicio.CrearSesion(usuario);
        }

        // "Juan@Mail.com " y "juan@mail.com" son la misma cuenta.
        private static string NormalizarEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
