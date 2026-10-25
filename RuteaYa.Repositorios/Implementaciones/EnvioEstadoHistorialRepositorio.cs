using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios.Implementaciones
{
    public class EnvioEstadoHistorialRepositorio : Repositorio<EnvioEstadoHistorial>, IEnvioEstadoHistorialRepositorio
    {
        public EnvioEstadoHistorialRepositorio(RuteaYaDbContext context) : base(context)
        {
        }

        public async Task<List<EnvioEstadoHistorial>> ObtenerPorEnvioAsync(int envioId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(h => h.EnvioId == envioId)
                .OrderBy(h => h.Fecha)
                .ToListAsync();
        }
    }
}
