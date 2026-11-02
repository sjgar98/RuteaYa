namespace RuteaYa.DTOs
{
    // Resumen de la pantalla de inicio (Pantalla 2).
    public class DashboardDto
    {
        // Salidas de viajes activos en los próximos 7 días (contando hoy si todavía no salió).
        public int CantidadProximasSalidas { get; set; }
        public List<ProximaSalidaDto> ProximasSalidas { get; set; } = new List<ProximaSalidaDto>();

        // Envíos propios que todavía no se entregaron ni se cancelaron.
        public int CantidadEnviosPendientes { get; set; }
        public List<EnvioDto> EnviosPendientes { get; set; } = new List<EnvioDto>();

        // Envíos distintos que entran en al menos uno de los viajes activos.
        public int CantidadOportunidades { get; set; }

        // Estimado: suma de la combinación óptima de cada viaje activo, contando solo los envíos
        // que se llevarían este mes y sin repetir un envío entre viajes. No es ganancia obtenida.
        public decimal PotencialMes { get; set; }
        public int PaquetesPotencialMes { get; set; }

        public int ViajesActivos { get; set; }
    }
}
