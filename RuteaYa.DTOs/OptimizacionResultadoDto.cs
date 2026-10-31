namespace RuteaYa.DTOs
{
    // Resultado completo del optimizador para una pantalla 10.
    public class OptimizacionResultadoDto
    {
        public int ViajeId { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public int MaxPaquetes { get; set; }
        public decimal PesoMaxKg { get; set; }
        public decimal DesvioMaxKm { get; set; }
        public DateOnly FechaViaje { get; set; }

        // Criterio usado: "ganancia", "desvio" o "equilibrio".
        public string Criterio { get; set; } = string.Empty;

        // Combinación óptima hallada (vacía si no hay ninguna válida).
        public CombinacionDto Optima { get; set; } = new CombinacionDto();

        // Otras combinaciones evaluadas que también cumplen las restricciones, ordenadas por valor.
        public List<CombinacionDto> OtrasCombinaciones { get; set; } = new List<CombinacionDto>();

        // Envíos que fueron descartados del análisis y el motivo.
        public List<EnvioDescartadoDto> Descartados { get; set; } = new List<EnvioDescartadoDto>();
    }
}

