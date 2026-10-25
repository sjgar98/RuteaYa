using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios
{
    public class AsignacionRepositorio : IAsignacionRepositorio
    {
        private readonly RuteaYaDbContext _db;

        public AsignacionRepositorio(RuteaYaDbContext db)
        {
            _db = db;
        }

        // Paquetes que transporta un conductor ("Mis transportes" e "Historial").
        public async Task<List<Asignacion>> ListarPorConductorAsync(int conductorId)
        {
            return await _db.Asignaciones
                .AsNoTracking()
                .Include(a => a.Envio)
                .Include(a => a.Viaje)
                .Where(a => a.Viaje.UsuarioId == conductorId)
                .OrderByDescending(a => a.FechaViaje)
                .ToListAsync();
        }

        public async Task<Asignacion?> ObtenerPorIdAsync(int id)
        {
            return await _db.Asignaciones
                .Include(a => a.Envio)
                .Include(a => a.Viaje)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Guarda una o varias asignaciones juntas (aceptar un envío o confirmar una combinación).
        // Si otro conductor ya tomó alguno de los envíos, el índice único sobre EnvioId
        // hace fallar el guardado con DbUpdateException.
        public async Task AgregarAsync(List<Asignacion> asignaciones)
        {
            _db.Asignaciones.AddRange(asignaciones);
            await _db.SaveChangesAsync();
        }
    }
}
