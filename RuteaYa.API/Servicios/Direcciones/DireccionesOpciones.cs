namespace RuteaYa.API.Servicios.Direcciones
{
    /// <summary>Sección "Direcciones" de la configuración de la API. No necesita ninguna key.</summary>
    public class DireccionesOpciones
    {
        /// <summary>"Photon" (búsqueda real con OpenStreetMap) o "Fijo" (lista de 12 barrios).</summary>
        public string Proveedor { get; set; } = "Photon";

        /// <summary>Si Photon falla, responder con la lista fija y dejar un aviso en el log.</summary>
        public bool RespaldoAlFijo { get; set; } = true;

        public string PhotonUrl { get; set; } = "https://photon.komoot.io";

        public int LimiteResultados { get; set; } = 5;

        /// <summary>Solo se aceptan resultados de este país (el bbox llega a la costa uruguaya).</summary>
        public string CodigoPais { get; set; } = "AR";

        // Photon prioriza los resultados cercanos a este punto (Obelisco).
        public double SesgoLat { get; set; } = -34.6037;
        public double SesgoLng { get; set; } = -58.3816;

        /// <summary>minLng,minLat,maxLng,maxLat: Gran Buenos Aires y La Plata.</summary>
        public string Bbox { get; set; } = "-59.1,-35.1,-57.85,-34.2";

        public bool UsarPhoton()
        {
            return string.Equals(Proveedor, "Photon", StringComparison.OrdinalIgnoreCase);
        }
    }
}