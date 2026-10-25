using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Enums;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios.Implementaciones
{
    public class EnvioRepositorio : Repositorio<Envio>, IEnvioRepositorio
    {
        public EnvioRepositorio(RuteaYaDbContext context) : base(context)
        {
        }

        public async Task<List<Envio>> ObtenerPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(e => e.Asignacion)
                .Where(e => e.UsuarioId == usuarioId);

            if (estado.HasValue)
            {
                query = query.Where(e => e.Estado == estado.Value);
            }

            return await query.OrderByDescending(e => e.CreadoEn).ToListAsync();
        }

        public async Task<Envio?> ObtenerConDetalleAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Asignacion)
                    .ThenInclude(a => a!.Viaje)
                .Include(e => e.Historial.OrderBy(h => h.Fecha))
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Envio>> ObtenerDisponiblesAsync(DateOnly desde, int? excluirUsuarioId = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(e => e.Estado == EstadoEnvio.BuscandoConductor
                         && e.Asignacion == null
                         && e.FechaLimite >= desde);

            if (excluirUsuarioId.HasValue)
            {
                query = query.Where(e => e.UsuarioId != excluirUsuarioId.Value);
            }

            return await query.OrderBy(e => e.FechaLimite).ToListAsync();
        }

        public async Task<List<Envio>> ObtenerPorIdsAsync(IEnumerable<int> ids)
        {
            var lista = ids.Distinct().ToList();
            return await _dbSet.Where(e => lista.Contains(e.Id)).ToListAsync();
        }
    }
}
