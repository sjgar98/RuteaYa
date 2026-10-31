namespace RuteaYa.DTOs
{
    // Una combinación de envíos evaluada por el optimizador.
    public class CombinacionDto
    {
        public List<OportunidadDto> Envios { get; set; } = new List<OportunidadDto>();
        public decimal GananciaTotal { get; set; }
        public decimal PesoTotalKg { get; set; }
        public decimal DesvioTotalKm { get; set; }
        public int CantidadPaquetes { get; set; }

        // Valor calculado según el criterio elegido (para mostrar en la UI qué se optimizó).
        public decimal Valor { get; set; }
    }
}

