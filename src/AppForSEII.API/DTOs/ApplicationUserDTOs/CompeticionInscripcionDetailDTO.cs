using System;
using System.Collections.Generic;

namespace AppForSEII.Shared.CompetitionDTOs
{
    public class CompeticionInscripcionDetailDTO
    {
        public CompeticionInscripcionDetailDTO(
            int id, 
            DateTime fechaInscripcion, 
            string nombreUsuario, 
            string apellidosUsuario, 
            string dni, 
            string telefono, 
            string datosPago, 
            MetodoPagoTypes metodoPago, 
            decimal precioTotal, 
            IList<CompeticionInscritaItemDTO> competicionesInscritas)
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
            CompeticionesInscritas = competicionesInscritas;
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
        public IList<CompeticionInscritaItemDTO> CompeticionesInscritas { get; set; }
    }

    public class CompeticionInscritaItemDTO
    {
        public CompeticionInscritaItemDTO(int competicionId, string nombre, string tipoDeporte, DateTime fecha, string lugar, decimal precio, string? observaciones)
        {
            CompeticionId = competicionId;
            Nombre = nombre;
            TipoDeporte = tipoDeporte;
            Fecha = fecha;
            Lugar = lugar;
            Precio = precio;
            Observaciones = observaciones;
        }

        public int CompeticionId { get; set; }
        public string Nombre { get; set; }
        public string TipoDeporte { get; set; }
        public DateTime Fecha { get; set; }
        public string Lugar { get; set; }
        public decimal Precio { get; set; }
        public string? Observaciones { get; set; }
    }

    public enum MetodoPagoTypes
    {
        Bizum,
        Efectivo,
        TarjetaCreditoDebito,
        Transferencia,
        Metalico
    }
}