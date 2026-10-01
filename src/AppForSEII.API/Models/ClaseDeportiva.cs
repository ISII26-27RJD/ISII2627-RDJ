using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    /// Clase que representa la entidad ClaseDeportiva del caso de uso "Apuntarse a clase deportiva".
    
    public class ClaseDeportiva
    {
        [Key]
        public int Id { get; set; }

        // Descripción de la clase deportiva, no puede estar vacía
        [Required]
        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        // Fecha y hora en que se imparte la clase
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        // Lugar donde se imparte la clase (puede ser nulo si está por definir)
        [StringLength(100, ErrorMessage = "El lugar no puede superar los 100 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Lugar")]
        public string? Lugar { get; set; }

        // Nombre del monitor que imparte la clase
        [Required]
        [StringLength(100, ErrorMessage = "El nombre del monitor no puede superar los 100 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Monitor")]
        public string Monitor { get; set; }

        // Nivel de la clase (ej. Principiante, Intermedio, Avanzado)
        [Required]
        [StringLength(50, ErrorMessage = "El nivel no puede superar los 50 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nivel")]
        public string Nivel { get; set; }

        // Las plazas disponibles no pueden ser negativas
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Las plazas disponibles no pueden ser negativas.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Plazas Disponibles")]
        public int PlazasDisponibles { get; set; }

        // Precio por persona, debe ser positivo, con 8 cifras significativas y 2 decimales
        [Required] 
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)] // Indica que es un campo de tipo moneda
        [Precision(8, 2)]   // Configuración de precisión para el precio unitario 8 cifras significativas y 2 decimales
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que 0.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Precio Unitario")] 
        public decimal PrecioUnitario { get; set; } 

        // Clave foránea hacia TipoDeporte
        [Required]
        public int TipoDeporteId { get; set; }

        // Propiedades de navegación (hacer más adelante las relaciones)

        public TipoDeporte TipoDeporte { get; set; }

        public IList<ClaseInscrita> ClasesInscritas { get; set; }
    }
}
