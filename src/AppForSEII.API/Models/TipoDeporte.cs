using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class TipoDeporte
    {
        // Constructor vacío requerido por Entity Framework Core
        public TipoDeporte()
        {
        }

        // Constructor con parámetros para crear instancias desde código
        public TipoDeporte(string nombre, string nombreTipoDeporte, string? descripcion = null)
        {
            Nombre            = nombre;
            NombreTipoDeporte = nombreTipoDeporte;
            Descripcion       = descripcion;
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de deporte es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del tipo de deporte no puede superar los 50 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Tipo de deporte")]
        public string Nombre { get; set; }

        // Nombre completo/descriptivo del tipo de deporte
        [Required(ErrorMessage = "El nombre del tipo de deporte es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del tipo de deporte no puede superar los 100 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre Tipo Deporte")]
        public string NombreTipoDeporte { get; set; }

        // Descripción opcional del tipo de deporte
        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Descripción")]
        public string? Descripcion { get; set; }


        // --- RELACIONES SEGÚN EL DIAGRAMA DE CLASES ---

        public IList<Competicion> Competiciones { get; set; }

        // Navegación caso de uso "Apuntarse a clase deportiva"
        public IList<ClaseDeportiva> ClasesDeportivas { get; set; }

        public IList<Material> Material{get; set;}
    }
}