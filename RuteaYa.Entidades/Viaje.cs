namespace RuteaYa.Entidades
{
    public class Viaje
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        public string OrigenDir { get; set; } = string.Empty;
        public decimal OrigenLat { get; set; }
        public decimal OrigenLng { get; set; }

        public string DestinoDir { get; set; } = string.Empty;
        public decimal DestinoLat { get; set; }
        public decimal DestinoLng { get; set; }

        public TimeOnly HoraSalida { get; set; }
        public TimeOnly HoraLlegada { get; set; }

        public int MaxPaquetes { get; set; }
        public decimal PesoMaxKg { get; set; }
        public decimal DesvioMaxKm { get; set; }

        // Distancia y duración de la ruta habitual, calculadas por el servicio de rutas.
        public decimal DistanciaKm { get; set; }
        public int DuracionMin { get; set; }

        public bool Activo { get; set; } = true;

        public Usuario Usuario { get; set; } = null!;
        public List<Parada> Paradas { get; set; } = new List<Parada>();
        public List<ViajeDia> Dias { get; set; } = new List<ViajeDia>();
        public List<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    }
}
