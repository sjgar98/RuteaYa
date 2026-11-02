using System.Globalization;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios.Direcciones
{
    /// <summary>Lista fija de barrios de CABA/GBA: respaldo cuando no hay red o Photon falla.</summary>
    public class DireccionServicioFijo : IDireccionServicio
    {
        public Task<List<DireccionDto>> BuscarAsync(string texto, CancellationToken cancelacion = default)
        {
            var direcciones = DireccionesDePrueba()
                .Where(d => CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                    d.Direccion, texto.Trim(), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
                .ToList();

            return Task.FromResult(direcciones);
        }

        private static List<DireccionDto> DireccionesDePrueba()
        {
            return new List<DireccionDto>
            {
                new DireccionDto { Direccion = "Belgrano, CABA", Lat = -34.562700m, Lng = -58.456500m },
                new DireccionDto { Direccion = "Palermo, CABA", Lat = -34.588900m, Lng = -58.430600m },
                new DireccionDto { Direccion = "Plaza Italia, Palermo, CABA", Lat = -34.581000m, Lng = -58.420800m },
                new DireccionDto { Direccion = "Recoleta, CABA", Lat = -34.587500m, Lng = -58.397400m },
                new DireccionDto { Direccion = "Obelisco, CABA", Lat = -34.603700m, Lng = -58.381600m },
                new DireccionDto { Direccion = "Caballito, CABA", Lat = -34.618900m, Lng = -58.442200m },
                new DireccionDto { Direccion = "Barracas, CABA", Lat = -34.645300m, Lng = -58.382600m },
                new DireccionDto { Direccion = "Avellaneda, Buenos Aires", Lat = -34.662700m, Lng = -58.365300m },
                new DireccionDto { Direccion = "Lomas de Zamora, Buenos Aires", Lat = -34.760900m, Lng = -58.406300m },
                new DireccionDto { Direccion = "Quilmes, Buenos Aires", Lat = -34.720600m, Lng = -58.254600m },
                new DireccionDto { Direccion = "Morón, Buenos Aires", Lat = -34.653400m, Lng = -58.619800m },
                new DireccionDto { Direccion = "San Isidro, Buenos Aires", Lat = -34.470800m, Lng = -58.528600m }
            };
        }
    }
}