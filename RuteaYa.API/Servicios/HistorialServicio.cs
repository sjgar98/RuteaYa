using System.Globalization;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class HistorialServicio : IHistorialServicio
    {
        public const string TipoViaje = "Viaje";
        public const string TipoEnvio = "Envio";

        private static readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("es-AR");
        private static readonly TimeSpan _offsetArgentina = TimeSpan.FromHours(-3);

        private readonly IAsignacionRepositorio _asignacionRepositorio;
        private readonly IEnvioRepositorio _envioRepositorio;

        public HistorialServicio(IAsignacionRepositorio asignacionRepositorio, IEnvioRepositorio envioRepositorio)
        {
            _asignacionRepositorio = asignacionRepositorio;
            _envioRepositorio = envioRepositorio;
        }

        public async Task<HistorialDto> ObtenerHistorialAsync(int usuarioId, DateOnly? desde, DateOnly? hasta, string? orden, string? estado)
        {
            var entregadas = await _asignacionRepositorio.ListarEntregadasPorConductorAsync(usuarioId, desde, hasta);
            var filtro = estado?.Trim().ToLowerInvariant();

            var items = new List<HistorialItemDto>();
            if (filtro is null or "" or "todos" or "viajes")
            {
                items.AddRange(ItemsDeViajes(entregadas));
            }

            if (filtro is null or "" or "todos" or "entregados" or "cancelados")
            {
                var finalizados = await _envioRepositorio.ListarFinalizadosPorUsuarioAsync(usuarioId);
                items.AddRange(finalizados
                    .Where(e => filtro is not "entregados" || e.Estado == EstadoEnvio.Entregado)
                    .Where(e => filtro is not "cancelados" || e.Estado == EstadoEnvio.Cancelado)
                    .Select(AItemDeEnvio)
                    .Where(i => (!desde.HasValue || i.Fecha >= desde.Value) && (!hasta.HasValue || i.Fecha <= hasta.Value)));
            }

            return new HistorialDto
            {
                ViajesRealizados = entregadas.Select(a => (a.ViajeId, a.FechaViaje)).Distinct().Count(),
                EnviosTransportados = entregadas.Count,
                GananciaEfectiva = entregadas.Sum(a => a.Ganancia),
                Items = Ordenar(items, orden)
            };
        }

        // Un ítem por viaje y mes, como en el mockup ("Septiembre · 3 paquetes").
        private static IEnumerable<HistorialItemDto> ItemsDeViajes(List<Asignacion> entregadas)
        {
            return entregadas
                .GroupBy(a => (a.ViajeId, a.FechaViaje.Year, a.FechaViaje.Month))
                .Select(grupo =>
                {
                    var viaje = grupo.First().Viaje;
                    var paquetes = grupo.Count();
                    var mes = _cultura.TextInfo.ToTitleCase(grupo.First().FechaViaje.ToString("MMMM", _cultura));

                    return new HistorialItemDto
                    {
                        Id = viaje.Id,
                        Tipo = TipoViaje,
                        Descripcion = $"{viaje.OrigenDir} → {viaje.DestinoDir}",
                        Detalle = $"{mes} · {paquetes} {(paquetes == 1 ? "paquete" : "paquetes")}",
                        Monto = grupo.Sum(a => a.Ganancia),
                        Fecha = grupo.Max(a => a.FechaViaje)
                    };
                });
        }

        private static HistorialItemDto AItemDeEnvio(Envio envio)
        {
            var cierre = envio.Historial
                .Where(h => h.EstadoNuevo == envio.Estado)
                .Select(h => (DateTime?)h.Fecha)
                .Max() ?? envio.CreadoEn;

            return new HistorialItemDto
            {
                Id = envio.Id,
                Tipo = TipoEnvio,
                Descripcion = $"{envio.OrigenDir} → {envio.DestinoDir}",
                Detalle = envio.Descripcion,
                Monto = envio.Monto,
                Fecha = DateOnly.FromDateTime(cierre.Add(_offsetArgentina)),
                Estado = envio.Estado
            };
        }

        private static List<HistorialItemDto> Ordenar(List<HistorialItemDto> items, string? orden)
        {
            return orden switch
            {
                "fecha_asc" => items.OrderBy(i => i.Fecha).ThenBy(i => i.Id).ToList(),
                "monto_asc" => items.OrderBy(i => i.Monto).ThenByDescending(i => i.Fecha).ToList(),
                "monto_desc" => items.OrderByDescending(i => i.Monto).ThenByDescending(i => i.Fecha).ToList(),
                _ => items.OrderByDescending(i => i.Fecha).ThenByDescending(i => i.Id).ToList()
            };
        }
    }
}
