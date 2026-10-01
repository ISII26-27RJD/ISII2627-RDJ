namespace AppForSEII.API.Models
{
    /// <summary>
    /// Enumerado que representa los métodos de pago disponibles al realizar una inscripción.
    /// </summary>
    public enum MetodoPago
    {
        Bizum,
        Efectivo,
        Tarjeta,
        Transferencia,
        Metalico
    }
}
