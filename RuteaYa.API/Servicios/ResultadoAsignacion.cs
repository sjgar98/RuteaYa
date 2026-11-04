using RuteaYa.DTOs;

namespace RuteaYa.API.Servicios
{
    // Resultado de aceptar uno o varios envíos: además del ResultadoOperacion lleva el motivo del conflicto
    // (para mostrárselo al conductor) o las asignaciones creadas.
    public class ResultadoAsignacion
    {
        public ResultadoOperacion Resultado { get; set; }
        public string? Mensaje { get; set; }
        public List<AsignacionDto> Asignaciones { get; set; } = new List<AsignacionDto>();
        public AsignacionDto? Asignacion => Asignaciones.FirstOrDefault();

        public static ResultadoAsignacion NoEncontrado()
        {
            return new ResultadoAsignacion { Resultado = ResultadoOperacion.NoEncontrado };
        }

        public static ResultadoAsignacion Conflicto(string mensaje)
        {
            return new ResultadoAsignacion { Resultado = ResultadoOperacion.Conflicto, Mensaje = mensaje };
        }

        public static ResultadoAsignacion Exito(List<AsignacionDto> asignaciones)
        {
            return new ResultadoAsignacion { Resultado = ResultadoOperacion.Exito, Asignaciones = asignaciones };
        }
    }
}
