namespace AppForSEII.Shared.ClassDTOs
{
    public class ClaseDeportivaInscripcionDetailDTO
    {
        public ClaseDeportivaInscripcionDetailDTO(int id, DateTime fechaInscripcion, string nombreUsuario, string apellidosUsuario, string dni, string telefono, string datosPago, MetodoPagoTypes metodoPago, decimal precioTotal, IList<ClassInscriptionItemDTO> clasesInscritas)
        {
            Id = id;
            FechaInscripcion = fechaInscripcion;
            NombreUsuario = nombreUsuario;
            ApellidosUsuario = apellidosUsuario;
            Dni = dni;
            Telefono = telefono;
            DatosPago = datosPago;
            MetodoPago = metodoPago;
            PrecioTotal = precioTotal;
            ClasesInscritas = clasesInscritas;
        }

        public int Id { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public string NombreUsuario { get; set; }
        public string ApellidosUsuario { get; set; }
        public string Dni { get; set; }
        public string Telefono { get; set; }
        public string DatosPago { get; set; }
        public MetodoPagoTypes MetodoPago { get; set; }
        public decimal PrecioTotal { get; set; }
        public IList<ClassInscriptionItemDTO> ClasesInscritas { get; set; }
    }

    public class ClassInscriptionItemDTO
    {
        public ClassInscriptionItemDTO(int claseDeportivaId, string descripcion, string tipoDeporte, DateTime fechaHora, int plazasReservadas, decimal precio, string? observaciones)
        {
            ClaseDeportivaId = claseDeportivaId;
            Descripcion = descripcion;
            TipoDeporte = tipoDeporte;
            FechaHora = fechaHora;
            PlazasReservadas = plazasReservadas;
            Precio = precio;
            Observaciones = observaciones;
        }

        public int ClaseDeportivaId { get; set; }
        public string Descripcion { get; set; }
        public string TipoDeporte { get; set; }
        public DateTime FechaHora { get; set; }
        public int PlazasReservadas { get; set; }
        public decimal Precio { get; set; }
        public string? Observaciones { get; set; }
    }

    public enum MetodoPagoTypes
    {
        Bizum,
        Efectivo,
        Tarjeta,
        Transferencia,
        Metálico
    }
}