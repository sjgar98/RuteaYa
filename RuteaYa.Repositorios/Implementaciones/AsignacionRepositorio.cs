using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Enums;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios.Implementaciones
{
    public class AsignacionRepositorio : Repositorio<Asignacion>, IAsignacionRepositorio
    {
        public AsignacionRepositorio(RuteaYaDbContext context) : base(context)
        {
        }

        public async Task<List<Asignacion>> ObtenerPorViajeAsync(int viajeId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(a => a.Envio)
                .Where(a => a.ViajeId == viajeId)
                .OrderBy(a => a.FechaViaje)
                .ToListAsync();
        }

        public async Task<List<Asignacion>> ObtenerPorConductorAsync(
            int conductorId,
            IEnumerable<EstadoEnvio>? estados = null,
            DateOnly? desde = null,
            DateOnly? hasta = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(a => a.Envio)
                .Include(a => a.Viaje)
                .Where(a => a.Viaje.UsuarioId == conductorId);

            if (estados != null)
            {
                var lista = estados.ToList();
                query = query.Where(a => lista.Contains(a.Envio.Estado));
            }

            if (desde.HasValue)
            {
                query = query.Where(a => a.FechaViaje >= desde.Value);
            }

            if (hasta.HasValue)
            {
                query = query.Where(a => a.FechaViaje <= hasta.Value);
            }

            return await query.OrderByDescending(a => a.FechaViaje).ToListAsync();
        }

        public async Task<Asignacion?> ObtenerConDetalleAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Envio)
                .Include(a => a.Viaje)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> ExisteParaEnvioAsync(int envioId)
        {
            return await _dbSet.AnyAsync(a => a.EnvioId == envioId);
        }
    }
}
