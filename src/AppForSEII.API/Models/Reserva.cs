using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppForSEII.API.Models
{
    /// <summary>
    /// Entidad que representa la reserva general realizada por un cliente.
    /// </summary>
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        // RELACIÓN CON EL CLIENTE (Sustituye a los campos sueltos de Nombre, Apellidos y DNI)
        [Required]
        public string ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public ApplicationUser Cliente { get; set; }

        // La fecha de la reserva debe ser en formato dd/MM/yyyy y no puede ser una fecha futura.
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaReserva { get; set; }

        // El texto del CU indica que será Bizum, Efectivo, Tarjeta, etc.
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public MetodoPago MetodoPago { get; set; }

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(5, 2)]
        public decimal PrecioTotal { get; set; }

        // RELACIÓN 1:N -> Una reserva contiene el desglose de múltiples pistas seleccionadas
        public IList<PistaReservada> PistasReservadas { get; set; } = new List<PistaReservada>();

        // CONSTRUCTORES
        public Reserva() { } // Requerido por EF Core

        public Reserva(string clienteId, DateTime fechaReserva, MetodoPago metodoPago, decimal precioTotal)
        {
            ClienteId = clienteId;
            FechaReserva = fechaReserva;
            MetodoPago = metodoPago;
            PrecioTotal = precioTotal;
        }
    }
}