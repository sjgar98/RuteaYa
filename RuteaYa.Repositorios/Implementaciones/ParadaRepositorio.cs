using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios.Implementaciones
{
    public class ParadaRepositorio : Repositorio<Parada>, IParadaRepositorio
    {
        public ParadaRepositorio(RuteaYaDbContext context) : base(context)
        {
        }

        public async Task<List<Parada>> ObtenerPorViajeAsync(int viajeId)
        {
            return await _dbSet
                .Where(p => p.ViajeId == viajeId)
                .OrderBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<Parada?> ObtenerDeViajeAsync(int viajeId, int paradaId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.ViajeId == viajeId && p.Id == paradaId);
        }
    }
}
