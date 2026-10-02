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


        // RELACIÓN N:1 -> Múltiples pistas pertenecen a un TipoDeporte
        [Required]
        public int IdTipoDeporte { get; set; }
        public TipoDeporte TipoDeporte { get; set; }

        // RELACIÓN 1:N -> Una pista puede tener múltiples historiales de reserva (PistaReservada)
        public IList<PistaReservada> PistasReservadas { get; set; } = new List<PistaReservada>();

        // CONSTRUCTORES
        public Pista() { } // Requerido por EF Core

        public Pista(string nombrePista, int nPersonas, decimal precio, int stock, int idTipoDeporte)
        {
            NombrePista = nombrePista;
            NPersonas = nPersonas;
            Precio = precio;
            Stock = stock;
            IdTipoDeporte = idTipoDeporte;
        }

    }
}