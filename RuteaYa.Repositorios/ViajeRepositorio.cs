using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios
{
    public class ViajeRepositorio : IViajeRepositorio
    {
        private readonly RuteaYaDbContext _db;

        public ViajeRepositorio(RuteaYaDbContext db)
        {
            _db = db;
        }

        public async Task<List<Viaje>> ListarPorUsuarioAsync(int usuarioId, bool? activo = null)
        {
            var query = _db.Viajes
                .AsNoTracking()
                .Include(v => v.Dias)
                .Where(v => v.UsuarioId == usuarioId);

            // null devuelve todos (activos e inactivos)
            if (activo.HasValue)
            {
                query = query.Where(v => v.Activo == activo.Value);
            }

            return await query.OrderBy(v => v.HoraSalida).ToListAsync();
        }

        // Viaje con días, paradas y asignaciones. Las paradas y los días se agregan,
        // modifican o eliminan desde estas colecciones y luego se llama a GuardarCambiosAsync.
        public async Task<Viaje?> ObtenerPorIdAsync(int id)
        {
            return await _db.Viajes
                .Include(v => v.Dias)
                .Include(v => v.Paradas.OrderBy(p => p.Orden))
                .Include(v => v.Asignaciones)
                    .ThenInclude(a => a.Envio)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AgregarAsync(Viaje viaje)
        {
            _db.Viajes.Add(viaje);
            await _db.SaveChangesAsync();
        }

        public async Task GuardarCambiosAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
