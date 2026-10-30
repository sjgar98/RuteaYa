using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>Cálculos geográficos simples, sin llamar a ningún proveedor.</summary>
    public static class GeoUtil
    {
        private const double RadioTierraKm = 6371.0;

        /// <summary>Distancia en línea recta (Haversine) entre dos puntos, en km.</summary>
        public static decimal DistanciaKm(CoordenadaDto a, CoordenadaDto b)
        {
            double lat1 = Radianes(a.Latitud);
            double lat2 = Radianes(b.Latitud);
            double dLat = lat2 - lat1;
            double dLng = Radianes(b.Longitud) - Radianes(a.Longitud);

            double h = Math.Pow(Math.Sin(dLat / 2), 2)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLng / 2), 2);

            return (decimal)(2 * RadioTierraKm * Math.Asin(Math.Min(1, Math.Sqrt(h))));
        }

        /// <summary>Largo en línea recta de un recorrido punto a punto, en km.</summary>
        public static decimal LongitudKm(IReadOnlyList<CoordenadaDto> recorrido)
        {
            decimal total = 0;
            for (int i = 0; i < recorrido.Count - 1; i++)
            {
                total += DistanciaKm(recorrido[i], recorrido[i + 1]);
            }
            return total;
        }

        /// <summary>
        /// Distancia del punto al vértice más cercano de la ruta. Sirve de prefiltro barato
        /// (etapa 1 del motor de compatibilidad): si ya está más lejos que el desvío máximo, se descarta.
        /// </summary>
        public static decimal DistanciaMinimaARutaKm(CoordenadaDto punto, IReadOnlyList<CoordenadaDto> ruta)
        {
            if (ruta.Count == 0)
            {
                return decimal.MaxValue;
            }
            return ruta.Min(v => DistanciaKm(punto, v));
        }

        /// <summary>Agrega puntos intermedios en línea recta; el mock los usa para tener algo que dibujar.</summary>
        public static List<CoordenadaDto> Densificar(IReadOnlyList<CoordenadaDto> recorrido, int pasosPorTramo)
        {
            var resultado = new List<CoordenadaDto>();
            for (int i = 0; i < recorrido.Count - 1; i++)
            {
                var a = recorrido[i];
                var b = recorrido[i + 1];
                for (int p = 0; p < pasosPorTramo; p++)
                {
                    decimal t = (decimal)p / pasosPorTramo;
                    resultado.Add(new CoordenadaDto(
                        a.Latitud + (b.Latitud - a.Latitud) * t,
                        a.Longitud + (b.Longitud - a.Longitud) * t));
                }
            }
            if (recorrido.Count > 0)
            {
                resultado.Add(recorrido[^1]);
            }
            return resultado;
        }

        private static double Radianes(decimal grados)
        {
            return (double)grados * Math.PI / 180.0;
        }
    }
}