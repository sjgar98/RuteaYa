using RuteaYa.DTOs;
using RuteaYa.Repositorios.Interfaces;
using RuteaYa.API.Servicios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class HistorialServicio : IHistorialServicio
    {
        private readonly IAsignacionRepositorio _asignacionRepositorio;
        private readonly IViajeRepositorio _viajeRepositorio;

        public HistorialServicio(IAsignacionRepositorio asignacionRepositorio, IViajeRepositorio viajeRepositorio)
        {
            _asignacionRepositorio = asignacionRepositorio;
            _viajeRepositorio = viajeRepositorio;
        }

        public async Task<HistorialDto> ObtenerHistorialAsync(int usuarioId, DateTime? desde, DateTime? hasta, string? orden, string? estado)
        {
            var asignacionesList = await _asignacionRepositorio.ListarEntregadasPorConductorAsync(usuarioId, desde, hasta);
            var totalViajesInactivos = (await _viajeRepositorio.ListarPorUsuarioAsync(usuarioId, false)).Count;

            int viajesRealizadosCount = asignacionesList.Select(a => a.ViajeId).Distinct().Count();
            int totalViajes = Math.Max(viajesRealizadosCount, totalViajesInactivos);
            int totalEnvios = asignacionesList.Count;
            decimal gananciaEfectiva = asignacionesList.Sum(a => a.Ganancia);

            var items = new List<HistorialItemDto>();

            var gruposPorViaje = asignacionesList.GroupBy(a => a.ViajeId);
            foreach (var grupo in gruposPorViaje)
            {
                var viaje = grupo.First().Viaje;
                var ultimaFecha = grupo.Max(a => a.AsignadoEn);
                var gananciaTotalViaje = grupo.Sum(a => a.Ganancia);
                var cantidadPaquetes = grupo.Count();

                string mesStr = ultimaFecha.ToString("MMMM");
                mesStr = char.ToUpper(mesStr[0]) + mesStr.Substring(1);

                items.Add(new HistorialItemDto
                {
                    Id = viaje.Id,
                    Tipo = "Viaje",
                    Descripcion = $"{viaje.OrigenDir} → {viaje.DestinoDir}",
                    Detalle = $"{mesStr} · {cantidadPaquetes} paquetes",
                    Monto = gananciaTotalViaje,
                    Fecha = ultimaFecha
                });
            }

     
            if (!string.IsNullOrEmpty(estado))
            {
                items = items.Where(i => i.Tipo.Equals(estado, StringComparison.OrdinalIgnoreCase)).ToList();
            }


            items = orden switch
            {
                "fecha_asc" => items.OrderBy(i => i.Fecha).ToList(),
                "fecha_desc" => items.OrderByDescending(i => i.Fecha).ToList(),
                "monto_asc" => items.OrderBy(i => i.Monto).ToList(),
                "monto_desc" => items.OrderByDescending(i => i.Monto).ToList(),
                _ => items.OrderByDescending(i => i.Fecha).ToList()
            };

            return new HistorialDto
            {
                ViajesRealizados = totalViajes,
                EnviosTransportados = totalEnvios,
                GananciaEfectiva = gananciaEfectiva,
                Items = items
            };
        }
    }
}