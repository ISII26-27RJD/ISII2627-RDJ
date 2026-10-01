using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    public class Inscripcion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del usuario no puede superar los 50 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre del usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "Los apellidos del usuario son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos del usuario no pueden superar los 100 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Apellidos del usuario")]
        public string ApellidosUsuario { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^[0-9]{8}[A-Za-z]$", ErrorMessage = "El DNI debe tener un formato válido (8 números y 1 letra).")]
        [System.ComponentModel.DataAnnotations.Display(Name = "DNI")]
        public string DNI { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Teléfono de contacto")]
        public string Telefono { get; set; }

        [Required]
        [DataTypeAttribute(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha de inscripción")]
        public DateTime FechaInscripcion { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Método de pago")]
        public string MetodoPago { get; set; }

        [Required]
        [Range(0.0, 999.99, ErrorMessage = "El precio total debe ser positivo.")]
        [DataTypeAttribute(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(5, 2)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Precio Total")]
        public decimal PrecioTotal { get; set; }

        // --- Propiedad de navegación según la relación del diagrama ---
        public IList<CompeticionInscrita> CompeticionesInscritas { get; set; }
    }
}