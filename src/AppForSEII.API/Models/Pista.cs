namespace AppForSEII.API.Models
{
    public class Pista
    {
        [Key]
        public int IdPista { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "El nombre de la pista no puede superar los 50 caracteres.")] 
        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre de la pista")]
        public string NombrePista { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El aforo debe ser al menos de 1 persona.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Aforo (Nº Personas)")]
        public int NPersonas { get; set; }

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(5, 2)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

    }
}