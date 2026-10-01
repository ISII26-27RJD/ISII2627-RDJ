using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    
    public class CompeticionInscrita
    {
        // --- PROPIEDADES / ATRIBUTOS DEL DIAGRAMA ---

        [Key]
        public int Id { get; set; } // O clave primaria/compuesta según la configuración del proyecto

        // Claves Foráneas (Foreign Keys)
        [Required]
        public int CompeticionId { get; set; }

        [Required]
        public int InscripcionId { get; set; }

        // Campo opcional según la especificación del caso de uso (Paso 5)
        [StringLength(250, ErrorMessage = "La descripción de problemas físicos no puede superar los 250 caracteres.")]
        [Display(Name = "Problemas físicos / Observaciones")]
        public string? ProblemasFisicos { get; set; }
        

        [ForeignKey(nameof(InscripcionId))]
        public Inscripcion Inscripcion { get; set; }

        
        [ForeignKey(nameof(CompeticionId))]
        public Competicion Competicion { get; set; }
    }
}