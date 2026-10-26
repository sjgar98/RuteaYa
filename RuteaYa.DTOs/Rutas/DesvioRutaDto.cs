using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.DTOs.Rutas
{
    public class DesvioRutaDto
    {
        public decimal DistanciaHabitualKm { get; set; }
        public decimal DistanciaConEnvioKm { get; set; }
        public decimal DesvioKm { get; set; }
        public int TiempoExtraMinutos { get; set; }
    }
}
