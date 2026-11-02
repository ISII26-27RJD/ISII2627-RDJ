using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.Shared.CompetitionDTOs
{
    public class CompeticionInscripcionForCreateDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string ApellidosUsuario { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Los datos de pago son obligatorios.")]
        public string DatosPago { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public MetodoPagoTypes MetodoPago { get; set; }

        public IList<CompeticionInscritaCreateItemDTO> CompeticionesInscritas { get; set; } = new List<CompeticionInscritaCreateItemDTO>();
    }

    public class CompeticionInscritaCreateItemDTO
    {
        [Required]
        public int CompeticionId { get; set; }

        public string? ProblemasFisicos { get; set; }
    }
}