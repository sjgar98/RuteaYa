using Microsoft.EntityFrameworkCore;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios.Implementaciones
{
    public class Repositorio<T> : IRepositorio<T> where T : class
    {
        protected readonly RuteaYaDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repositorio(RuteaYaDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> ObtenerPorIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<List<T>> ObtenerTodosAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task AgregarAsync(T entidad)
        {
            await _dbSet.AddAsync(entidad);
        }

        public void Actualizar(T entidad)
        {
            _dbSet.Update(entidad);
        }

        public void Eliminar(T entidad)
        {
            _dbSet.Remove(entidad);
        }

        // El DbContext es scoped, así que un solo guardado confirma los cambios de todos los repositorios del request.
        public Task<int> GuardarCambiosAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
