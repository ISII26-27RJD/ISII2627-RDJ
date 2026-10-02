namespace AppForSEII.API.Models
{
    /// <summary>
    /// Clase que representa la entidad Pista del caso de uso "Reservar pista".
    /// </summary>
    public class Pista
    {
        [Key] //Esta clave no se llama ID sin más, es necesario usar anotación
        public int IdPista { get; set; }

        //Limito tamaño en base de datos a 50 caracteres, defino error en caso de no cumplirse esto
        [Required]
        [StringLength(50, ErrorMessage = "El nombre de la pista no puede superar los 50 caracteres.")] 
        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre de la pista")]
        public string NombrePista { get; set; }

        //Necesito al menos una persona reservando una pista
        [Required]
        [Range(1, 40, ErrorMessage = "El aforo debe ser al menos de 1 persona.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Aforo (Nº Personas)")]
        public int NPersonas { get; set; }

        //Precio de la pista debe ser positivo, con 5 cifras significativas, y dos decimales (ej. 996.12)
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(5, 2)]
        public decimal Precio { get; set; }

        //Puedo no elegir material porque tengo el mío, o si eligo, no puede ser cantidades negativas
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        // Hacer más adelante las relaciones

        // public TipoDeporte TipoDeporte { get; set; }
        // public IList<PistaReservada> PistasReservadas { get; set; }

    }
}