using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class TipoDeporte
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de deporte es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del tipo de deporte no puede superar los 50 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Tipo de deporte")]
        public string Nombre { get; set; }

        // --- RELACIONES SEGÚN EL DIAGRAMA DE CLASES ---

        
        public IList<Competicion> Competiciones { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class TipoDeporte
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de deporte es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del tipo de deporte no puede superar los 50 caracteres.")]
        [Display(Name = "Tipo de deporte")]
        public string Nombre { get; set; }

        // --- RELACIONES SEGÚN EL DIAGRAMA DE CLASES ---

        
        public IList<Competicion> Competiciones { get; set; }
    }
}