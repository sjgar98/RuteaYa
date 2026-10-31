using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>
    /// Decide dónde insertar el retiro y la entrega de un envío en el recorrido habitual.
    /// Prueba todas las posiciones válidas (retiro siempre antes que la entrega, nunca después del
    /// destino ni antes del origen) y se queda con la de menor largo en línea recta. Sin llamar a Google.
    /// </summary>
    public static class InsercionRecorrido
    {
        public static List<CoordenadaDto> Insertar(
            IReadOnlyList<CoordenadaDto> recorrido, CoordenadaDto retiro, CoordenadaDto entrega)
        {
            if (recorrido.Count < 2)
            {
                throw new ArgumentException("El recorrido necesita al menos origen y destino.", nameof(recorrido));
            }

            int tramos = recorrido.Count - 1;
            List<CoordenadaDto>? mejor = null;
            decimal mejorLongitud = decimal.MaxValue;

            for (int i = 0; i < tramos; i++)
            {
                for (int j = i; j < tramos; j++)
                {
                    var candidato = Construir(recorrido, retiro, entrega, i, j);
                    decimal longitud = GeoUtil.LongitudKm(candidato);

                    // Con "<" estricto, ante un empate gana la primera posición: resultado determinístico.
                    if (longitud < mejorLongitud)
                    {
                        mejorLongitud = longitud;
                        mejor = candidato;
                    }
                }
            }

            return mejor!;
        }

        // Retiro después del punto i y entrega después del punto j (i <= j). Si i == j van seguidos.
        private static List<CoordenadaDto> Construir(
            IReadOnlyList<CoordenadaDto> recorrido, CoordenadaDto retiro, CoordenadaDto entrega, int i, int j)
        {
            var resultado = new List<CoordenadaDto>();
            for (int k = 0; k < recorrido.Count; k++)
            {
                resultado.Add(recorrido[k]);
                if (k == i)
                {
                    resultado.Add(retiro);
                }
                if (k == j)
                {
                    resultado.Add(entrega);
                }
            }
            return resultado;
        }
    }
}