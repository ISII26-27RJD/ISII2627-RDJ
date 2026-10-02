

using DisplayAttribute = System.ComponentModel.DataAnnotations.DisplayAttribute;

namespace AppForSEII.API.Models
{
    
    public class Alquiler
    {
        
        public Alquiler(){}

        [Key]
        [Display(Name = "IdAlquiler")]
        public int IdAlquiler{get; set;}

        [Required]
        [Display(Name = "NombreUsuario")]
        [StringLength(50, ErrorMessage = "Nombre entre 3 y 50 caracteres",MinimumLength = 3)]
        public string NombreUsuario{get; set;}

        [Required]
        [Display(Name = "ApellidosUsuario")]
        [StringLength(50, ErrorMessage = "Nombre entre 3 y 50 caracteres",MinimumLength = 3)]
        public string ApellidosUsuario{get; set;}

        [Required]
        [Display(Name = "DNI")]
        [StringLength(9, ErrorMessage = "DNI debe tener 9 caracteres",MinimumLength = 9)]
        [RegularExpression("[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][A-Za-z]")]
        public string DNI{get; set;}

        [Required]
        [Phone]
        [Display(Name = "NumeroTelefono")]
        public string NumeroTelefono{get; set;}

        [Required]
        [Display(Name = "FechaAlquiler")]
        public DateTime FechaAlquiler{get; set;}

        [Required]
        [Display(Name = "FechaInicioAlquiler")]
        public DateTime FechaInicioAlquiler{set; get;}

        [Required]
        [Display(Name = "FechaFinAlquiler")]
        public DateTime FechaFinAlquiler{get; set;}

        [Required]
        [Display(Name = "PrecioTotal")]
        [Range(1,200,ErrorMessage ="Minimo 1")]
        public decimal PrecioTotal{get; set;}

        [Required]
        [Display(Name = "MetodoPago")]
        public MetodoPagoTipos MetodoPago{get; set;}

        public List<MaterialAlquilado> MaterialesAlquilados{get; set;}
        
        public enum MetodoPagoTipos
        {
            Bizum,
            Efectivo,
            Tarjeta,
            Transferencia
        }
    }
}