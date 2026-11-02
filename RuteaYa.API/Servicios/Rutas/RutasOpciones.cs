namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>Sección "Rutas" de la configuración de la API.</summary>
    public class RutasOpciones
    {
        /// <summary>
        /// "Auto" (Google si hay key; si no, OSRM), "Google", "Osrm" o "Mock".
        /// </summary>
        public string Proveedor { get; set; } = "Auto";

        /// <summary>Key de servidor (Routes API). Opcional: va en user-secrets, no en appsettings.</summary>
        public string GoogleApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Si el proveedor real falla (red, cuota, servicio caído), responder con el mock y dejar un aviso en el log.
        /// Con el mock los desvíos y tiempos son simulados: apagarlo para comprobar que el proveedor anda de verdad.
        /// </summary>
        public bool RespaldoAlMock { get; set; } = true;

        /// <summary>Servidor de OSRM. El público (demo) no pide key pero es de uso moderado.</summary>
        public string OsrmUrl { get; set; } = "https://router.project-osrm.org";

        /// <summary>Tiempo mínimo entre dos consultas a OSRM: el servidor público pide 1 request por segundo.</summary>
        public int OsrmIntervaloMs { get; set; } = 1000;

        public bool UsarGoogle()
        {
            bool hayKey = !string.IsNullOrWhiteSpace(GoogleApiKey);

            if (string.Equals(Proveedor, "Google", StringComparison.OrdinalIgnoreCase))
            {
                if (!hayKey)
                {
                    throw new InvalidOperationException(
                        "Rutas:Proveedor es \"Google\" pero falta Rutas:GoogleApiKey (user-secrets de RuteaYa.API).");
                }
                return true;
            }

            return string.Equals(Proveedor, "Auto", StringComparison.OrdinalIgnoreCase) && hayKey;
        }

        public bool UsarOsrm()
        {
            if (string.Equals(Proveedor, "Osrm", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            bool hayKey = !string.IsNullOrWhiteSpace(GoogleApiKey);
            return string.Equals(Proveedor, "Auto", StringComparison.OrdinalIgnoreCase) && !hayKey;
        }
    }
}