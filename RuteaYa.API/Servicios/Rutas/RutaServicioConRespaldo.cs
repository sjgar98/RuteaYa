using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>
    /// Decorador de IRutaServicio: usa el proveedor principal (Google) y, si falla, responde con el
    /// respaldo (mock) dejando un aviso en el log. Los errores de programación (argumentos inválidos)
    /// y la cancelación del llamador no se tapan.
    /// </summary>
    public class RutaServicioConRespaldo : IRutaServicio
    {
        private const string ClaveProveedorCaido = "rutas:proveedor-caido";
        private static readonly TimeSpan PausaTrasFalloDeRed = TimeSpan.FromSeconds(60);

        private readonly IRutaServicio _principal;
        private readonly IRutaServicio _respaldo;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RutaServicioConRespaldo> _logger;

        public RutaServicioConRespaldo(
            IRutaServicio principal,
            IRutaServicio respaldo,
            IMemoryCache cache,
            ILogger<RutaServicioConRespaldo> logger)
        {
            _principal = principal;
            _respaldo = respaldo;
            _cache = cache;
            _logger = logger;
        }

        public Task<RutaResultadoDto> CalcularRutaAsync(IReadOnlyList<CoordenadaDto> recorrido, CancellationToken cancelacion = default)
        {
            return EjecutarAsync(nameof(CalcularRutaAsync), cancelacion, p => p.CalcularRutaAsync(recorrido, cancelacion));
        }

        public Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(
            IReadOnlyList<CoordenadaDto> recorridoHabitual, CoordenadaDto retiro, CoordenadaDto entrega, CancellationToken cancelacion = default)
        {
            return EjecutarAsync(nameof(CompararConEnvioAsync), cancelacion,
                p => p.CompararConEnvioAsync(recorridoHabitual, retiro, entrega, cancelacion));
        }

        public Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion)
        {
            return EjecutarAsync(nameof(GeocodificarDireccionAsync), default, p => p.GeocodificarDireccionAsync(direccion));
        }

        

        public Task<RutaResultadoDto> CalcularRutaConParadasAsync(List<string> paradas)
        {
            return EjecutarAsync(nameof(CalcularRutaConParadasAsync), default, p => p.CalcularRutaConParadasAsync(paradas));
        }

        public Task<RutaResultadoDto> CalcularRutaAlternativaEnvioAsync(string origen, string destino, string retiroPaquete, string entregaPaquete)
        {
            return EjecutarAsync(nameof(CalcularRutaAlternativaEnvioAsync), default,
                p => p.CalcularRutaAlternativaEnvioAsync(origen, destino, retiroPaquete, entregaPaquete));
        }

        public Task<decimal> CalcularDesvioAsync(string rutaOriginalId, string paradaExtra)
        {
            return EjecutarAsync(nameof(CalcularDesvioAsync), default, p => p.CalcularDesvioAsync(rutaOriginalId, paradaExtra));
        }

        public Task<DetalleOportunidadRutaDto> ObtenerDetalleOportunidadAsync(
            string origenHabitual, string destinoHabitual, string retiroEnvio, string entregaEnvio)
        {
            return EjecutarAsync(nameof(ObtenerDetalleOportunidadAsync), default,
                p => p.ObtenerDetalleOportunidadAsync(origenHabitual, destinoHabitual, retiroEnvio, entregaEnvio));
        }

        public Task<List<string>> BuscarDireccionesAsync(string texto)
        {
            return EjecutarAsync(nameof(BuscarDireccionesAsync), default, p => p.BuscarDireccionesAsync(texto));
        }

        private async Task<T> EjecutarAsync<T>(string operacion, CancellationToken cancelacion, Func<IRutaServicio, Task<T>> accion)
        {
            if (_cache.TryGetValue(ClaveProveedorCaido, out _))
            {
                _logger.LogDebug("Proveedor de rutas en pausa: {Operacion} se resuelve con el respaldo.", operacion);
                return await accion(_respaldo);
            }

            try
            {
                return await accion(_principal);
            }
            catch (Exception ex) when (EsFalloDelProveedor(ex, cancelacion))
            {
                if (EsFalloDeRed(ex, cancelacion))
                {
                    _cache.Set(ClaveProveedorCaido, true, PausaTrasFalloDeRed);
                }

                _logger.LogWarning(ex,
                    "Falló el proveedor de rutas en {Operacion}: se usa el respaldo (mock). Distancias, desvíos y tiempos son simulados.",
                    operacion);

                return await accion(_respaldo);
            }
        }

        private static bool EsFalloDelProveedor(Exception ex, CancellationToken cancelacion)
        {
            return ex is RutaNoDisponibleException
                || ex is JsonException
                || ex is KeyNotFoundException
                || EsFalloDeRed(ex, cancelacion);
        }

        private static bool EsFalloDeRed(Exception ex, CancellationToken cancelacion)
        {
            return ex is HttpRequestException
                || (ex is TaskCanceledException && !cancelacion.IsCancellationRequested);
        }
    }
}