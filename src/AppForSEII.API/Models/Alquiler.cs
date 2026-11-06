

using DisplayAttribute = System.ComponentModel.DataAnnotations.DisplayAttribute;

namespace AppForSEII.API.Models
{
    
    public class Alquiler
    {
        
        public Alquiler(){}

        public Alquiler(ApplicationUser usuario, DateTime fechaA, IList<MaterialAlquilado> materialAlquilados, MetodoPagoTipos metodoPago)
        {

            Usuario = usuario;
            FechaAlquiler = fechaA;
            MaterialesAlquilados = materialAlquilados;
            MetodoPago = metodoPago;

            PrecioTotal = decimal.Round(materialAlquilados.Sum(pi => pi.Precio * pi.Cantidad), 2);
            
        }

        [Key]
        [Display(Name = "IdAlquiler")]
        public int IdAlquiler{get; set;}

        [Required]
        public ApplicationUser Usuario{get; set;}

        [Required]
        [Display(Name = "FechaAlquiler")]
        public DateTime FechaAlquiler{get; set;}

        [Required]
        [Display(Name = "PrecioTotal")]
        [Range(1,200,ErrorMessage ="Minimo 1")]
        public decimal PrecioTotal{get; set;}

        [Required]
        [Display(Name = "MetodoPago")]
        public MetodoPagoTipos MetodoPago{get; set;}

        public IList<MaterialAlquilado> MaterialesAlquilados{get; set;}
        
        
        public enum MetodoPagoTipos
        {
            Bizum,
            Efectivo,
            Tarjeta,
            Transferencia
        }
    }
}