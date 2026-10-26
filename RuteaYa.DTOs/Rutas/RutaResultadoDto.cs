using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.DTOs.Rutas
{
    public class RutaResultadoDto
    {
        public decimal DistanciaKm { get; set; }
        public int DuracionMinutos { get; set; }
        public List<CoordenadaDto> PuntosRecorrido { get; set; } = new List<CoordenadaDto>();
        public string Polyline { get; set; } = string.Empty; // Sirve para pintar la linea en el mapa
    }
}
