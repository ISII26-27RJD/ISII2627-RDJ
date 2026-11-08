using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForSEII.Shared.ClassDTOs
{
    // DTO de respuesta que devuelve el detalle completo de una inscripcion a una clase deportiva.
    // Se usa tanto en el GET Details como en la respuesta del POST al crear una inscripcion.
    public class ClaseDeportivaInscripcionDetailDTO
    {
        // Constructor que inicializa todos los campos del DTO con los datos de la inscripcion.
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

        // Identificador unico de la inscripcion en la base de datos.
        public int Id { get; set; }
        // Fecha y hora en la que se realizo la inscripcion.
        public DateTime FechaInscripcion { get; set; }
        // Nombre del usuario que se ha inscrito.
        public string NombreUsuario { get; set; }
        // Apellidos del usuario que se ha inscrito.
        public string ApellidosUsuario { get; set; }
        // DNI del usuario inscrito.
        public string Dni { get; set; }
        // Telefono de contacto del usuario.
        public string Telefono { get; set; }
        // Datos del pago (p.ej. numero de tarjeta enmascarado o referencia de Bizum).
        public string DatosPago { get; set; }
        // Metodo de pago utilizado (Bizum, Tarjeta, etc.), usando el enum del shared.
        public MetodoPagoTypes MetodoPago { get; set; }
        // Precio total de la inscripcion, calculado sumando el precio de todas las ClasesInscritas.
        public decimal PrecioTotal { get; set; }
        // Lista de las clases deportivas a las que se ha inscrito el usuario en esta inscripcion.
        public IList<ClassInscriptionItemDTO> ClasesInscritas { get; set; }

        // Sobreescribimos Equals para poder comparar dos DTOs por valor en los tests.
        // Sin esto, la comparacion por defecto seria por referencia de objeto (siempre falso entre dos objetos distintos).
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
                   // Compara las listas elemento a elemento con SequenceEqual
                   (ClasesInscritas == null && other.ClasesInscritas == null ||
                    ClasesInscritas != null && other.ClasesInscritas != null && ClasesInscritas.SequenceEqual(other.ClasesInscritas));
        }

        // GetHashCode debe sobreescribirse siempre que se sobreescriba Equals.
        // Genera un codigo hash a partir de todos los campos relevantes.
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

    // DTO que representa cada una de las clases deportivas dentro de una inscripcion (linea de detalle).
    // Es el equivalente a una linea de un ticket: que clase, cuantas plazas y a que precio.
    public class ClassInscriptionItemDTO
    {
        // Constructor que inicializa todos los datos de la linea de inscripcion.
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

        // ID de la clase deportiva a la que pertenece esta linea.
        public int ClaseDeportivaId { get; set; }
        // Descripcion/nombre de la clase deportiva.
        public string Descripcion { get; set; }
        // Nombre del tipo de deporte (p.ej. "Padel", "Tenis").
        public string TipoDeporte { get; set; }
        // Fecha y hora de celebracion de la clase.
        public DateTime FechaHora { get; set; }
        // Numero de plazas reservadas en esta clase dentro de la inscripcion.
        public int PlazasReservadas { get; set; }
        // Precio total pagado por estas plazas (PrecioUnitario * PlazasReservadas).
        public decimal Precio { get; set; }
        // Observaciones opcionales del alumno para esta clase (puede ser null).
        public string? Observaciones { get; set; }

        // Equals por valor para poder usar Assert.Equal en los tests.
        // Compara la fecha solo por dia (Date) para ignorar la hora exacta.
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

        // GetHashCode combinando todos los campos relevantes usando HashCode.Combine.
        public override int GetHashCode()
        {
            return HashCode.Combine(ClaseDeportivaId, Descripcion, TipoDeporte, FechaHora.Date, PlazasReservadas, Precio, Observaciones);
        }
    }

    // Enum que define los metodos de pago disponibles en el sistema compartido (AppForSEII.Shared).
    // Es un espejo del enum MetodoPago del lado del API, necesario para no exponer el modelo de dominio directamente.
    public enum MetodoPagoTypes
    {
        Bizum,
        Efectivo,
        Tarjeta,
        Transferencia,
        Metálico
    }
}
