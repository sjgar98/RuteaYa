using System.Text;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>Algoritmo de "encoded polyline" de Google (precisión 1e5).</summary>
    public static class PolylineCodec
    {
        public static string Codificar(IEnumerable<CoordenadaDto> puntos)
        {
            var texto = new StringBuilder();
            long latAnterior = 0;
            long lngAnterior = 0;

            foreach (var punto in puntos)
            {
                long lat = (long)Math.Round(punto.Latitud * 100000m, MidpointRounding.AwayFromZero);
                long lng = (long)Math.Round(punto.Longitud * 100000m, MidpointRounding.AwayFromZero);
                Escribir(texto, lat - latAnterior);
                Escribir(texto, lng - lngAnterior);
                latAnterior = lat;
                lngAnterior = lng;
            }

            return texto.ToString();
        }

        public static List<CoordenadaDto> Decodificar(string codificado)
        {
            var puntos = new List<CoordenadaDto>();
            int indice = 0;
            long lat = 0;
            long lng = 0;

            while (indice < codificado.Length)
            {
                lat += Leer(codificado, ref indice);
                lng += Leer(codificado, ref indice);
                puntos.Add(new CoordenadaDto(lat / 100000m, lng / 100000m));
            }

            return puntos;
        }

        private static void Escribir(StringBuilder texto, long valor)
        {
            long v = valor < 0 ? ~(valor << 1) : valor << 1;
            while (v >= 0x20)
            {
                texto.Append((char)((0x20 | (v & 0x1f)) + 63));
                v >>= 5;
            }
            texto.Append((char)(v + 63));
        }

        private static long Leer(string texto, ref int indice)
        {
            long resultado = 0;
            int desplazamiento = 0;
            int bloque;
            do
            {
                bloque = texto[indice++] - 63;
                resultado |= (long)(bloque & 0x1f) << desplazamiento;
                desplazamiento += 5;
            } while (bloque >= 0x20);

            return (resultado & 1) != 0 ? ~(resultado >> 1) : resultado >> 1;
        }
    }
}