using RuteaYa.Entidades;
using RuteaYa.Entidades.Enums;

namespace RuteaYa.Repositorios.Interfaces
{
    public interface IEnvioRepositorio : IRepositorio<Envio>
    {
        /// <param name="estado">null devuelve los envíos en cualquier estado.</param>
        Task<List<Envio>> ObtenerPorUsuarioAsync(int usuarioId, EstadoEnvio? estado = null);

        /// <summary>Envío con su asignación (y viaje asociado) e historial de estados.</summary>
        Task<Envio?> ObtenerConDetalleAsync(int id);

        /// <summary>
        /// Envíos en estado "Buscando conductor" cuya fecha límite no es anterior a <paramref name="desde"/>.
        /// Excluye los publicados por <paramref name="excluirUsuarioId"/> (el propio conductor).
        /// </summary>
        Task<List<Envio>> ObtenerDisponiblesAsync(DateOnly desde, int? excluirUsuarioId = null);

        Task<List<Envio>> ObtenerPorIdsAsync(IEnumerable<int> ids);
    }
}
