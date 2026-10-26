namespace RuteaYa.API.Servicios
{
    // Resultado de una operación que modifica un recurso; el controller lo traduce a 2xx / 404 / 409.
    public enum ResultadoOperacion
    {
        Exito = 1,
        NoEncontrado = 2,
        Conflicto = 3
    }
}
