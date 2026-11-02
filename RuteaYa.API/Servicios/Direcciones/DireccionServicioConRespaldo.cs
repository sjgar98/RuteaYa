using Microsoft.Extensions.Caching.Memory;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Direcciones
{
    /// <summary>
    /// Usa el proveedor principal (Photon) y, si falla, responde con el respaldo (lista fija) dejando un aviso en el log.
    /// Tras un fallo se pausa al principal un rato: el servicio público puede estar throttleando y cada tecla
    /// del autocompletado esperaría el timeout.
    /// </summary>
    public class DireccionServicioConRespaldo : IDireccionServicio
    {
        private const string ClaveProveedorCaido = "direcciones:proveedor-caido";
        private static readonly TimeSpan PausaTrasFallo = TimeSpan.FromSeconds(60);

        private readonly IDireccionServicio _principal;
        private readonly IDireccionServicio _respaldo;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DireccionServicioConRespaldo> _logger;

        public DireccionServicioConRespaldo(
            IDireccionServicio principal,
            IDireccionServicio respaldo,
            IMemoryCache cache,
            ILogger<DireccionServicioConRespaldo> logger)
        {
            _principal = principal;
            _respaldo = respaldo;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<DireccionDto>> BuscarAsync(string texto, CancellationToken cancelacion = default)
        {
            if (_cache.TryGetValue(ClaveProveedorCaido, out _))
            {
                return await _respaldo.BuscarAsync(texto, cancelacion);
            }

            try
            {
                return await _principal.BuscarAsync(texto, cancelacion);
            }
            catch (Exception ex) when (EsFalloDelProveedor(ex, cancelacion))
            {
                _cache.Set(ClaveProveedorCaido, true, PausaTrasFallo);
                _logger.LogWarning(ex,
                    "Falló la búsqueda de direcciones: se usa la lista fija durante {Segundos} s.", PausaTrasFallo.TotalSeconds);

                return await _respaldo.BuscarAsync(texto, cancelacion);
            }
        }

        // Un TaskCanceledException sin cancelación del llamador es el timeout del HttpClient.
        private static bool EsFalloDelProveedor(Exception ex, CancellationToken cancelacion)
        {
            return ex is DireccionesNoDisponiblesException
                || ex is HttpRequestException
                || (ex is TaskCanceledException && !cancelacion.IsCancellationRequested);
        }
    }
}