namespace RuteaYa.Entidades
{
    public class Parada
    {
        public int Id { get; set; }
        public int ViajeId { get; set; }
        public int Orden { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }

        public Viaje Viaje { get; set; } = null!;
    }
}
