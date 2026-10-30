using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>
    /// Compara la ruta habitual con la ruta que incluye un envío. Lo comparten el mock y Google:
    /// cada uno solo aporta cómo se calcula una ruta.
    /// Completa solo los datos de ruta; monto, peso, capacidad y ganancia por km los calcula OportunidadesServicio.
    /// </summary>
    public static class ComparadorRutas
    {
        public static async Task<DetalleOportunidadRutaDto> CompararAsync(
            Func<IReadOnlyList<CoordenadaDto>, CancellationToken, Task<RutaResultadoDto>> calcularRuta,
            IReadOnlyList<CoordenadaDto> recorridoHabitual,
            CoordenadaDto retiro,
            CoordenadaDto entrega,
            CancellationToken cancelacion)
        {
            // La habitual se repite entre candidatos: el proveedor la cachea por coordenadas.
            var habitual = await calcularRuta(recorridoHabitual, cancelacion);

            var recorridoConEnvio = InsercionRecorrido.Insertar(recorridoHabitual, retiro, entrega);
            var conEnvio = await calcularRuta(recorridoConEnvio, cancelacion);

            return new DetalleOportunidadRutaDto
            {
                DistanciaHabitualKm = habitual.DistanciaKm,
                DistanciaConEnvioKm = conEnvio.DistanciaKm,
                DesvioKm = Math.Max(0, conEnvio.DistanciaKm - habitual.DistanciaKm),
                TiempoHabitualMinutos = habitual.DuracionMinutos,
                TiempoAdicionalMinutos = Math.Max(0, conEnvio.DuracionMinutos - habitual.DuracionMinutos),
                RutaHabitual = habitual,
                RutaConEnvio = conEnvio
            };
        }
    }
}