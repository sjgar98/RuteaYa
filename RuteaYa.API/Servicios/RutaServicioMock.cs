using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs.Rutas;
using RuteaYa.API.Servicios.Rutas;

namespace RuteaYa.API.Servicios
{
    public class RutaServicioMock : IRutaServicio
    {
        private const decimal FactorCalles = 1.3m;        // la ruta real es más larga que la línea recta
        private const decimal VelocidadMediaKmH = 30m;

        public Task<RutaResultadoDto> CalcularRutaAsync(IReadOnlyList<CoordenadaDto> recorrido, CancellationToken cancelacion = default)
        {
            decimal km = Math.Round(GeoUtil.LongitudKm(recorrido) * FactorCalles, 2);
            var puntos = GeoUtil.Densificar(recorrido, 10);

            return Task.FromResult(new RutaResultadoDto
            {
                DistanciaKm = km,
                DuracionMinutos = (int)Math.Ceiling(km / VelocidadMediaKmH * 60m),
                PuntosRecorrido = puntos,
                Polyline = PolylineCodec.Codificar(puntos)
            });
        }

        public Task<DetalleOportunidadRutaDto> CompararConEnvioAsync(
            IReadOnlyList<CoordenadaDto> recorridoHabitual, CoordenadaDto retiro, CoordenadaDto entrega, CancellationToken cancelacion = default)
        {
            return ComparadorRutas.CompararAsync(CalcularRutaAsync, recorridoHabitual, retiro, entrega, cancelacion);
        }
    }
}
