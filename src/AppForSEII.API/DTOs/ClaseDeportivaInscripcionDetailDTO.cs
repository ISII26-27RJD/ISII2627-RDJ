using System;
using System.Collections.Generic;
using System.Linq;

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

        public override bool Equals(object? obj)
        {
            return obj is ClaseDeportivaInscripcionDetailDTO other &&
                   Id == other.Id &&
                   NombreUsuario == other.NombreUsuario &&
                   ApellidosUsuario == other.ApellidosUsuario &&
                   Dni == other.Dni &&
                   Telefono == other.Telefono &&
                   DatosPago == other.DatosPago &&
                   MetodoPago == other.MetodoPago &&
                   PrecioTotal == other.PrecioTotal &&
                   (ClasesInscritas == null && other.ClasesInscritas == null ||
                    ClasesInscritas != null && other.ClasesInscritas != null && ClasesInscritas.SequenceEqual(other.ClasesInscritas));
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(NombreUsuario);
            hash.Add(ApellidosUsuario);
            hash.Add(Dni);
            hash.Add(Telefono);
            hash.Add(DatosPago);
            hash.Add(MetodoPago);
            hash.Add(PrecioTotal);
            return hash.ToHashCode();
        }
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

        public override bool Equals(object? obj)
        {
            return obj is ClassInscriptionItemDTO other &&
                   ClaseDeportivaId == other.ClaseDeportivaId &&
                   Descripcion == other.Descripcion &&
                   TipoDeporte == other.TipoDeporte &&
                   FechaHora.Date == other.FechaHora.Date &&
                   PlazasReservadas == other.PlazasReservadas &&
                   Precio == other.Precio &&
                   Observaciones == other.Observaciones;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClaseDeportivaId, Descripcion, TipoDeporte, FechaHora.Date, PlazasReservadas, Precio, Observaciones);
        }
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
