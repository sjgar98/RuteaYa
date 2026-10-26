using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RuteaYa.DTOs.Rutas
{
    public class CalcularRutaConsultaDto
    {
        [Required(ErrorMessage = "El origen es obligatorio.")]
        public string Origen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El destino es obligatorio.")]
        public string Destino { get; set; } = string.Empty;
        public List<string> Paradas { get; set; } = new List<string>();
    }
}
