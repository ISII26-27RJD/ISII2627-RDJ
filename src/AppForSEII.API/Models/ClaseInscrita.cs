using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    public class ClaseInscrita
    {
        // Constructor vacío requerido por Entity Framework Core
        public ClaseInscrita()
        {
        }

        // Constructor con parámetros para crear instancias desde código
        public ClaseInscrita(int claseDeportivaId, int inscripcionId, int plazasReservadas, decimal precio, string? observaciones = null)
        {
            ClaseDeportivaId = claseDeportivaId;
            InscripcionId    = inscripcionId;
            PlazasReservadas = plazasReservadas;
            Precio           = precio;
            Observaciones    = observaciones;
        }

        // --- PROPIEDADES / ATRIBUTOS DEL DIAGRAMA ---

        [Key]
        public int Id { get; set; }

        // Claves Foráneas (Foreign Keys)
        [Required]
        public int ClaseDeportivaId { get; set; }

        [Required]
        public int InscripcionId { get; set; }

        // Observaciones opcionales del alumno sobre la clase inscrita
        [StringLength(250, ErrorMessage = "Las observaciones no pueden superar los 250 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Número de plazas reservadas, debe ser al menos 1
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Las plazas reservadas deben ser al menos 1.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Plazas Reservadas")]
        public int PlazasReservadas { get; set; }

        // Precio calculado en el momento de la inscripción, debe ser positivo
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Precision(8, 2)]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Precio")]
        public decimal Precio { get; set; }

        // --- RELACIONES DE NAVEGACIÓN SEGÚN EL DIAGRAMA DE CLASES ---

        [ForeignKey(nameof(InscripcionId))]
        public Inscripcion Inscripcion { get; set; }

        [ForeignKey(nameof(ClaseDeportivaId))]
        public ClaseDeportiva ClaseDeportiva { get; set; }
    }
}
