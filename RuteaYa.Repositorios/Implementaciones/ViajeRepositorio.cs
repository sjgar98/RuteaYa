using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios.Implementaciones
{
    public class ViajeRepositorio : Repositorio<Viaje>, IViajeRepositorio
    {
        public ViajeRepositorio(RuteaYaDbContext context) : base(context)
        {
        }

        public async Task<List<Viaje>> ObtenerPorUsuarioAsync(int usuarioId, bool? activo = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(v => v.Dias)
                .Include(v => v.Paradas.OrderBy(p => p.Orden))
                .Where(v => v.UsuarioId == usuarioId);

            if (activo.HasValue)
            {
                query = query.Where(v => v.Activo == activo.Value);
            }

            return await query.OrderBy(v => v.HoraSalida).ToListAsync();
        }

        public async Task<Viaje?> ObtenerConDetalleAsync(int id)
        {
            return await ConDetalle().FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Viaje?> ObtenerDeUsuarioAsync(int id, int usuarioId)
        {
            return await ConDetalle().FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == usuarioId);
        }

        private IQueryable<Viaje> ConDetalle()
        {
            return _dbSet
                .Include(v => v.Dias)
                .Include(v => v.Paradas.OrderBy(p => p.Orden))
                .Include(v => v.Asignaciones)
                    .ThenInclude(a => a.Envio)
                .AsSplitQuery();
        }
    }
}
