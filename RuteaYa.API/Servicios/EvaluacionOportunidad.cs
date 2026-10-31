using RuteaYa.DTOs.Rutas;

namespace RuteaYa.API.Servicios
{
    // Resultado de evaluar si un envío es compatible con un viaje.
    // Lo consumen OportunidadesServicio, OptimizadorServicio y tests.
    public class EvaluacionOportunidad
    {
        // null = compatible; con texto = motivo de rechazo.
        public string? Motivo { get; set; }

        // Primera fecha en que el viaje sale con lugar para el envío.
        public DateOnly? Fecha { get; set; }

        // Datos de la ruta con el envío insertado (null si no se pudo calcular o no es compatible).
        public DetalleOportunidadRutaDto? Ruta { get; set; }

        public bool EsCompatible => Motivo == null;

        public EvaluacionOportunidad ConMotivo(string motivo)
        {
            Motivo = motivo;
            return this;
        }
    }
}

