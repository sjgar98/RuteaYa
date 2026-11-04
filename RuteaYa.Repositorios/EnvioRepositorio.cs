using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.DTOs.Enums;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios
{
    public class EnvioRepositorio : IEnvioRepositorio
    {
        private readonly RuteaYaDbContext _db;

        public EnvioRepositorio(RuteaYaDbContext db)
        {
            _db = db;
        }

        public async Task<List<Envio>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null)
        {
            var query = _db.Envios
                .AsNoTracking()
                .Where(e => e.UsuarioId == usuarioId);

            // null devuelve los envíos en cualquier estado
            if (estado.HasValue)
            {
                query = query.Where(e => e.Estado == estado.Value);
            }

            return await query.OrderByDescending(e => e.CreadoEn).ToListAsync();
        }

        // Envíos que buscan conductor, no vencidos y que no publicó el propio conductor.
        public async Task<List<Envio>> ListarDisponiblesAsync(DateOnly desde, int usuarioIdExcluido)
        {
            return await _db.Envios
                .AsNoTracking()
                .Where(e => e.Estado == EstadoEnvio.BuscandoConductor
                         && e.FechaLimite >= desde
                         && e.UsuarioId != usuarioIdExcluido)
                .OrderBy(e => e.FechaLimite)
                .ToListAsync();
        }

        public async Task<List<Envio>> ListarPorIdsAsync(List<int> ids)
        {
            return await _db.Envios
                .Include(e => e.Asignacion)
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();
        }

        // Envío con su asignación (y viaje) e historial. Los cambios de estado se registran
        // agregando a Historial y luego se llama a GuardarCambiosAsync.
        public async Task<Envio?> ObtenerPorIdAsync(int id)
        {
            return await _db.Envios
                .Include(e => e.Asignacion)
                    .ThenInclude(a => a!.Viaje)
                .Include(e => e.Historial.OrderBy(h => h.Fecha))
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AgregarAsync(Envio envio)
        {
            _db.Envios.Add(envio);
            await _db.SaveChangesAsync();
        }

        public async Task GuardarCambiosAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
