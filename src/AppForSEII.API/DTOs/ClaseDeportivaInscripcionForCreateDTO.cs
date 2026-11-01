using System.ComponentModel.DataAnnotations;

namespace AppForSEII.Shared.ClassDTOs
{
    public class ClaseDeportivaInscripcionForCreateDTO
    {
        [Required]
        public string NombreUsuario { get; set; }
        [Required]
        public string ApellidosUsuario { get; set; }
        [Required]
        public string Dni { get; set; }
        [Required]
        public string Telefono { get; set; }
        [Required]
        public string DatosPago { get; set; }
        [Required]
        public MetodoPagoTypes MetodoPago { get; set; }

        public IList<ClassInscriptionCreateItemDTO> ClasesInscritas { get; set; } = new List<ClassInscriptionCreateItemDTO>();
    }

    public class ClassInscriptionCreateItemDTO
    {
        [Required]
        public int ClaseDeportivaId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int PlazasReservadas { get; set; }
        public string? Observaciones { get; set; }
    }
}