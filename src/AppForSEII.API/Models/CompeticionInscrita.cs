using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    
    public class CompeticionInscrita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompeticionId { get; set; }

        [Required]
        public int InscripcionId { get; set; }

        [StringLength(250, ErrorMessage = "La descripción de problemas físicos no puede superar los 250 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Problemas físicos / Observaciones")]
        public string? ProblemasFisicos { get; set; }

        // --- RELACIONES DE NAVEGACIÓN ---

        [ForeignKey(nameof(InscripcionId))]
        public Inscripcion Inscripcion { get; set; }

        [ForeignKey(nameof(CompeticionId))]
        public Competicion Competicion { get; set; }
    }
}