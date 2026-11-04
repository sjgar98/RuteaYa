namespace RuteaYa.DTOs
{
    // Operaciones anteriores del usuario (Pantalla 13). Los contadores son como conductor y solo cuentan
    // envíos entregados: es la ganancia efectiva, no la potencial.
    public class HistorialDto
    {
        public int ViajesRealizados { get; set; }
        public int EnviosTransportados { get; set; }
        public decimal GananciaEfectiva { get; set; }
        public List<HistorialItemDto> Items { get; set; } = new List<HistorialItemDto>();
    }
}
