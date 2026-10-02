using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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

        [Required]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string NombreCliente { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
        public string Apellidos { get; set; }

        [Required]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El DNI debe tener 9 caracteres (8 números y 1 letra).")]
        public string Dni { get; set; }

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaReserva { get; set; }

        // El texto del CU indica que será Bizum, Efectivo, Tarjeta, etc.
        [Required]
        [StringLength(30, ErrorMessage = "El método de pago no es válido.")]
        public string MetodoPago { get; set; }

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(5, 2)]
        public decimal PrecioTotal { get; set; }

        // TODO: Propiedades de navegación (Se implementarán al finalizar todas las clases)
        // public IList<PistaReservada> PistasReservadas { get; set; }
    }
}