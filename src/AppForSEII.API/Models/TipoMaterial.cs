using System.ComponentModel.DataAnnotations;
using DisplayAttribute = System.ComponentModel.DataAnnotations.DisplayAttribute;

namespace AppForSEII.API.Models
{
    
    public class TipoMaterial
    {
        public TipoMaterial(){}

        [Key]
        [Display(Name = "IdTipoMaterial")]
        public int IdTipoMaterial{get; set;}

        [Required]
        [Display(Name = "NombreTipoMaterial")]
        [StringLength(50, ErrorMessage = "Nombre del tipo entre 4 y 50 caracteres", MinimumLength = 4)]
        public string NombreTipoMaterial{get; set;}

        public List<Material> Material{get; set;} 
    }

}