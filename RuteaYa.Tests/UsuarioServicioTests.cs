using Microsoft.AspNetCore.Identity;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Tests
{
    public class UsuarioServicioTests
    {
        private readonly PasswordHasher<Usuario> _passwordHasher = new PasswordHasher<Usuario>();

        [Fact]
        public async Task RegistrarAsync_DatosValidos_GuardaUsuarioConHashYDevuelveSesion()
        {
            var repositorio = new UsuarioRepositorioFalso();
            var servicio = CrearServicio(repositorio);

            var sesion = await servicio.RegistrarAsync(CrearRegistro(" Juan@Mail.com "));

            Assert.NotNull(sesion);
            Assert.Equal("token-de-prueba", sesion.Token);
            var usuario = Assert.Single(repositorio.Usuarios);
            Assert.Equal("Juan", usuario.Nombre);
            Assert.Equal("Pérez", usuario.Apellido);
            Assert.Equal("juan@mail.com", usuario.Email);
            Assert.NotEqual("Clave1234", usuario.PasswordHash);
            Assert.Equal(PasswordVerificationResult.Success,
                _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, "Clave1234"));
        }

        [Fact]
        public async Task RegistrarAsync_EmailExistenteConOtrasMayusculas_DevuelveNullYNoGuarda()
        {
            var repositorio = new UsuarioRepositorioFalso();
            repositorio.Usuarios.Add(new Usuario { Id = 1, Email = "juan@mail.com" });
            var servicio = CrearServicio(repositorio);

            var sesion = await servicio.RegistrarAsync(CrearRegistro("JUAN@mail.com"));

            Assert.Null(sesion);
            Assert.Single(repositorio.Usuarios);
        }

        [Fact]
        public async Task LoginAsync_CredencialesCorrectas_DevuelveSesionDelUsuario()
        {
            var repositorio = new UsuarioRepositorioFalso();
            var servicio = CrearServicio(repositorio);
            await servicio.RegistrarAsync(CrearRegistro("juan@mail.com"));

            var sesion = await servicio.LoginAsync(new LoginDto { Email = "Juan@Mail.com", Password = "Clave1234" });

            Assert.NotNull(sesion);
            Assert.Equal("juan@mail.com", sesion.Email);
        }

        [Fact]
        public async Task LoginAsync_PasswordIncorrecta_DevuelveNull()
        {
            var repositorio = new UsuarioRepositorioFalso();
            var servicio = CrearServicio(repositorio);
            await servicio.RegistrarAsync(CrearRegistro("juan@mail.com"));

            var sesion = await servicio.LoginAsync(new LoginDto { Email = "juan@mail.com", Password = "Otra1234" });

            Assert.Null(sesion);
        }

        [Fact]
        public async Task LoginAsync_EmailInexistente_DevuelveNull()
        {
            var servicio = CrearServicio(new UsuarioRepositorioFalso());

            var sesion = await servicio.LoginAsync(new LoginDto { Email = "nadie@mail.com", Password = "Clave1234" });

            Assert.Null(sesion);
        }

        // Si alguien cambia la contraseña o el hash del seed sin actualizar el otro, no se podría iniciar sesión.
        [Fact]
        public void UsuarioPrueba_HashDelSeed_CorrespondeAPassword()
        {
            var resultado = _passwordHasher.VerifyHashedPassword(new Usuario(), UsuarioPrueba.PasswordHash, UsuarioPrueba.Password);

            Assert.Equal(PasswordVerificationResult.Success, resultado);
        }

        private UsuarioServicio CrearServicio(UsuarioRepositorioFalso repositorio)
        {
            return new UsuarioServicio(repositorio, _passwordHasher, new TokenServicioFalso());
        }

        private static RegistroUsuarioDto CrearRegistro(string email)
        {
            return new RegistroUsuarioDto
            {
                Nombre = " Juan ",
                Apellido = "Pérez",
                Email = email,
                Password = "Clave1234",
                ConfirmarPassword = "Clave1234"
            };
        }

        private class TokenServicioFalso : ITokenServicio
        {
            public SesionDto CrearSesion(Usuario usuario)
            {
                return new SesionDto { Token = "token-de-prueba", UsuarioId = usuario.Id, Email = usuario.Email };
            }
        }

        private class UsuarioRepositorioFalso : IUsuarioRepositorio
        {
            public List<Usuario> Usuarios { get; } = new List<Usuario>();

            public Task<Usuario?> ObtenerPorIdAsync(int id)
            {
                return Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
            }

            public Task<Usuario?> ObtenerPorEmailAsync(string email)
            {
                return Task.FromResult(Usuarios.FirstOrDefault(u => u.Email == email));
            }

            public Task<bool> ExisteEmailAsync(string email)
            {
                return Task.FromResult(Usuarios.Any(u => u.Email == email));
            }

            public Task AgregarAsync(Usuario usuario)
            {
                usuario.Id = Usuarios.Count + 1;
                Usuarios.Add(usuario);
                return Task.CompletedTask;
            }
        }
    }
}
