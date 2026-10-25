using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly RuteaYaDbContext _db;

        public UsuarioRepositorio(RuteaYaDbContext db)
        {
            _db = db;
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _db.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            return await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _db.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task AgregarAsync(Usuario usuario)
        {
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
        }
    }
}
