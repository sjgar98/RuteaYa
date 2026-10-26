using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RutasController : ControllerBase
    {
        private readonly IRutaServicio _rutaServicio;

        public RutasController(IRutaServicio rutaServicio)
        {
            _rutaServicio = rutaServicio;
        }

        /// <summary>Geocodifica una dirección de texto a coordenadas geográficas. </summary>
        [HttpGet("geocodificar")]
        [ProducesResponseType(typeof(CoordenadaDto), 200)]
        public async Task<IActionResult> Geocodificar([FromQuery] string direccion)
        {
            var resultado = await _rutaServicio.GeocodificarDireccionAsync(direccion);
            return Ok(resultado);
        }

        /// <summary> Calcula la ruta total, distancia y duración dadas unas paradas. </summary>
        [HttpPost("calcular")]
        [ProducesResponseType(typeof(RutaResultadoDto), 200)]
        public async Task<IActionResult> CalcularRuta([FromBody] CalcularRutaConsultaDto consulta)
        {
            var resultado = await _rutaServicio.CalcularRutaConParadasAsync(consulta.Paradas);
            return Ok(resultado);
        }

        /// <summary> Busca direcciones sugeridas para los campos de autocompletado.  </summary>
        [HttpGet("direcciones")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<IActionResult> BuscarDirecciones([FromQuery] string texto)
        {
            var resultado = await _rutaServicio.BuscarDireccionesAsync(texto);
            return Ok(resultado);
        }

        /// <summary> Obtiene el detalle comparativo de una oportunidad de envío frente a la ruta habitual. </summary>
        [HttpGet("oportunidad-detalle")]
        [ProducesResponseType(typeof(DetalleOportunidadRutaDto), 200)]
        public async Task<IActionResult> ObtenerDetalleOportunidad(
            [FromQuery] string origenHabitual,
            [FromQuery] string destinoHabitual,
            [FromQuery] string retiroEnvio,
            [FromQuery] string entregaEnvio)
        {
            var resultado = await _rutaServicio.ObtenerDetalleOportunidadAsync(origenHabitual, destinoHabitual, retiroEnvio, entregaEnvio);
            return Ok(resultado);
        }
    }
}