namespace RuteaYa.API.Servicios.Rutas
{
    /// <summary>Sección "Rutas" de la configuración de la API.</summary>
    public class RutasOpciones
    {
        /// <summary>"Auto" (Google si hay key, si no mock), "Google" o "Mock".</summary>
        public string Proveedor { get; set; } = "Auto";

        /// <summary>Key de servidor (Routes API + Geocoding API). Va en user-secrets, no en appsettings.</summary>
        public string GoogleApiKey { get; set; } = string.Empty;

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
    }
}