using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.DTOs.Rutas
{
    public class CoordenadaDto
    {
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }

        public CoordenadaDto() { }

        public CoordenadaDto(decimal latitud, decimal longitud)
        {
            Latitud = latitud;
            Longitud = longitud;
        }
    }
}
