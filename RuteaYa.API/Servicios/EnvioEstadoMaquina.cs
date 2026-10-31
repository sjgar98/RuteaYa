using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;

namespace RuteaYa.API.Servicios
{
    public static class EnvioEstadoMaquina
    {
        private static readonly Dictionary<EstadoEnvio, EstadoEnvio[]> Transiciones = new()
        {
            [EstadoEnvio.BuscandoConductor] = new[] { EstadoEnvio.ConductorAsignado, EstadoEnvio.Cancelado },
            [EstadoEnvio.ConductorAsignado] = new[] { EstadoEnvio.Retirado, EstadoEnvio.Cancelado },
            [EstadoEnvio.Retirado] = new[] { EstadoEnvio.EnCamino },
            [EstadoEnvio.EnCamino] = new[] { EstadoEnvio.Entregado },
            [EstadoEnvio.Entregado] = Array.Empty<EstadoEnvio>(),
            [EstadoEnvio.Cancelado] = Array.Empty<EstadoEnvio>()
        };

        public static bool PuedeTransicionar(EstadoEnvio actual, EstadoEnvio nuevo)
        {
            return Transiciones.TryGetValue(actual, out var permitidos) && permitidos.Contains(nuevo);
        }

        public static bool Transicionar(Envio envio, EstadoEnvio nuevo)
        {
            if (!PuedeTransicionar(envio.Estado, nuevo))
            {
                return false;
            }

            envio.Historial.Add(new EnvioEstadoHistorial
            {
                EstadoAnterior = envio.Estado,
                EstadoNuevo = nuevo,
                Fecha = DateTime.UtcNow
            });
            envio.Estado = nuevo;

            return true;
        }
    }
}