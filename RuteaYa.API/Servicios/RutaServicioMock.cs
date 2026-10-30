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
        private readonly List<string> _direccionesPrueba = new()
        {
            "Belgrano, CABA",
            "Lomas de Zamora, Buenos Aires",
            "Palermo, CABA",
            "San Isidro, Buenos Aires",
            "Plaza Italia, CABA",
            "Obelisco, CABA",
            "Recoleta, CABA",
            "Quilmes, Buenos Aires",
            "Avellaneda, Buenos Aires",
            "Caballito, CABA",
            "Morón, Buenos Aires",
            "Barrancas de Belgrano, CABA"
        };

        public async Task<CoordenadaDto> GeocodificarDireccionAsync(string direccion)
        {
            await Task.Delay(50);
            return new CoordenadaDto { Latitud = -34.6037m, Longitud = -58.3816m };
        }

        public async Task<RutaResultadoDto> CalcularRutaConParadasAsync(List<string> paradas)
        {
            await Task.Delay(100);
            int cantidadParadas = paradas?.Count ?? 0;
            return new RutaResultadoDto
            {
                DistanciaKm = 10.5m + (cantidadParadas * 2.5m),
                DuracionMinutos = 30 + (cantidadParadas * 10),
                PuntosRecorrido = new List<CoordenadaDto>
                {
                    new(-34.6037m, -58.3816m),
                    new(-34.6100m, -58.3900m)
                }
            };
        }

        public async Task<RutaResultadoDto> CalcularRutaAlternativaEnvioAsync(string origen, string destino, string retiro, string entrega)
        {
            await Task.Delay(100);
            return new RutaResultadoDto
            {
                DistanciaKm = 14.0m,
                DuracionMinutos = 45
            };
        }

        public async Task<decimal> CalcularDesvioAsync(string rutaOriginalId, string paradaExtra)
        {
            await Task.Delay(50);
            return 3.5m;
        }

        public async Task<DetalleOportunidadRutaDto> ObtenerDetalleOportunidadAsync(string origenHabitual, string destinoHabitual, string retiroEnvio, string entregaEnvio)
        {
            await Task.Delay(150);
            return new DetalleOportunidadRutaDto
            {
                DistanciaHabitualKm = 12.0m,
                DistanciaConEnvioKm = 15.5m,
                DesvioKm = 3.5m,
                TiempoHabitualMinutos = 30,
                TiempoAdicionalMinutos = 12,
                MontoPago = 4000.0m,
                GananciaPorKmAdicional = 1142.85m,
                PesoKg = 2.0m,
                CapacidadRestantePaquetes = 2,
                CapacidadRestanteKg = 8.0m
            };
        }

        public async Task<List<string>> BuscarDireccionesAsync(string texto)
        {
            await Task.Delay(30);
            if (string.IsNullOrWhiteSpace(texto))
                return _direccionesPrueba;

            return _direccionesPrueba
                .Where(d => d.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
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
