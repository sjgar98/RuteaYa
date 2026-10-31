using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios
{
    // Resultado de aceptar una oportunidad: además del ResultadoOperacion lleva el motivo del conflicto
    // (para mostrárselo al conductor) o la asignación creada.
    public class ResultadoAsignacion
    {
        public ResultadoOperacion Resultado { get; set; }
        public string? Mensaje { get; set; }
        public AsignacionDto? Asignacion { get; set; }

        public static ResultadoAsignacion NoEncontrado()
        {
            return new ResultadoAsignacion { Resultado = ResultadoOperacion.NoEncontrado };
        }

        public static ResultadoAsignacion Conflicto(string mensaje)
        {
            return new ResultadoAsignacion { Resultado = ResultadoOperacion.Conflicto, Mensaje = mensaje };
        }

        public static ResultadoAsignacion Exito(AsignacionDto asignacion)
        {
            return new ResultadoAsignacion { Resultado = ResultadoOperacion.Exito, Asignacion = asignacion };
        }
    }
}
