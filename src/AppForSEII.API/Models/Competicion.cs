using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    public class Competicion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la competición es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la competición no puede superar los 100 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre de la competición")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El lugar de la competición es obligatorio.")]
        [StringLength(100, ErrorMessage = "El lugar de la competición no puede superar los 100 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Lugar")]
        public string Lugar { get; set; }

        [Required(ErrorMessage = "La fecha de la competición es obligatoria.")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha y hora")]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Debe haber al menos 1 plaza disponible.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Plazas disponibles")]
        public int Plazas { get; set; }

        [Required]
        [Range(0.0, 999.99, ErrorMessage = "El precio debe ser positivo.")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(5, 2)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Precio de la inscripción")]
        public decimal Precio { get; set; }

        // Clave foránea y propiedad de navegación para la relación con TipoDeporte (Relación N:1)
        [Required]
        public int TipoDeporteId { get; set; }

        [ForeignKey(nameof(TipoDeporteId))]
        public TipoDeporte TipoDeporte { get; set; }

        // Relación 1:N con la entidad intermedia CompeticionInscrita
        public IList<CompeticionInscrita> CompeticionesInscritas { get; set; }
    }
}