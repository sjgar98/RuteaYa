using System.Globalization;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.MVC.Helpers
{
    // Textos y formatos que se repiten en las vistas.
    public static class EtiquetasHelper
    {
        private static readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("es-AR");

        public static string Abreviar(this DiaSemana dia)
        {
            return dia switch
            {
                DiaSemana.Lunes => "Lun",
                DiaSemana.Martes => "Mar",
                DiaSemana.Miercoles => "Mié",
                DiaSemana.Jueves => "Jue",
                DiaSemana.Viernes => "Vie",
                DiaSemana.Sabado => "Sáb",
                _ => "Dom"
            };
        }

        public static string Texto(this EstadoEnvio estado)
        {
            return estado switch
            {
                EstadoEnvio.BuscandoConductor => "Buscando conductor",
                EstadoEnvio.ConductorAsignado => "Conductor asignado",
                EstadoEnvio.Retirado => "Retirado",
                EstadoEnvio.EnCamino => "En camino",
                EstadoEnvio.Entregado => "Entregado",
                _ => "Cancelado"
            };
        }

        // "$4.000"
        public static string Moneda(this decimal monto)
        {
            return "$" + monto.ToString("N0", _cultura);
        }

        // "10", "3,5"
        public static string Numero(this decimal valor)
        {
            return valor.ToString("0.##", _cultura);
        }

        public static string Texto(this TamanoEnvio tamano)
        {
            return tamano switch
            {
                TamanoEnvio.Pequeno => "Pequeño",
                TamanoEnvio.Mediano => "Mediano",
                _ => "Grande"
            };
        }
        public static string ClaseBadge(this EstadoEnvio estado)
        {
            return estado switch
            {
                EstadoEnvio.BuscandoConductor => "text-bg-warning-subtle text-warning-emphasis",
                EstadoEnvio.ConductorAsignado => "text-bg-light border",
                EstadoEnvio.Retirado => "text-bg-light border",
                EstadoEnvio.EnCamino => "text-bg-info-subtle text-info-emphasis",
                EstadoEnvio.Entregado => "text-bg-success-subtle text-success-emphasis",
                _ => "text-bg-danger-subtle text-danger-emphasis"
            };
        }
        private static readonly TimeZoneInfo _zonaArgentina = TimeZoneInfo.CreateCustomTimeZone(
            "Argentina", TimeSpan.FromHours(-3), "Hora de Argentina", "Hora de Argentina");
        public static DateTime AHoraLocal(this DateTime fechaUtc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, _zonaArgentina);
        }
    }
}
