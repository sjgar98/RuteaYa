using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.DTOs.Rutas
{
    public class DetalleOportunidadRutaDto
    {
        public decimal DistanciaHabitualKm { get; set; }
        public decimal DistanciaConEnvioKm { get; set; }
        public decimal DesvioKm { get; set; }
        public int TiempoHabitualMinutos { get; set; }
        public int TiempoAdicionalMinutos { get; set; }
        public decimal MontoPago { get; set; }
        public decimal GananciaPorKmAdicional { get; set; }
        public decimal PesoKg { get; set; }
        public int CapacidadRestantePaquetes { get; set; }
        public decimal CapacidadRestanteKg { get; set; }
        public RutaResultadoDto RutaHabitual { get; set; } = new RutaResultadoDto();
        public RutaResultadoDto RutaConEnvio { get; set; } = new RutaResultadoDto();
    }
}
