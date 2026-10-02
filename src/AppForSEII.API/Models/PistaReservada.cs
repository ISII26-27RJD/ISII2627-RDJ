using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    /// <summary>
    /// Entidad de unión que representa el detalle de una pista específica dentro de una reserva.
    /// </summary>
    public class PistaReservada
    {
        //Esto es la clave primaria
        [Key]
        public int Id { get; set; }

        // Minimo una pista a reservar
        [Required]
        [Range(1, 5, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(5, 2)]
        public decimal Precio { get; set; }

        // Al ser observaciones, usamos nullable (string?) y tipo multilínea
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres.")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string? Observaciones { get; set; }

        // RELACIÓN N:1 -> Esta fila pertenece a UNA Pista concreta
        [Required]
        public int IdPista { get; set; }
        public Pista Pista { get; set; }

        // RELACIÓN N:1 -> Esta fila pertenece a UNA Reserva concreta
        [Required]
        public int IdReserva { get; set; }
        public Reserva Reserva { get; set; }

        // CONSTRUCTORES
        public PistaReservada() { } // Requerido por EF Core

        public PistaReservada(int cantidad, decimal precio, string? observaciones, int idPista, int idReserva)
        {
            Cantidad = cantidad;
            Precio = precio;
            Observaciones = observaciones;
            IdPista = idPista;
            IdReserva = idReserva;
        }

        
    }
}