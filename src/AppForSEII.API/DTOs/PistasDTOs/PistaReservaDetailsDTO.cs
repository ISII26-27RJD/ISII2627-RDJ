using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PistasReservaDetailsDTOs
{
    public class PistaReservaDetailsDTO
    {
        // Constructor vacío
        public PistaReservaDetailsDTO()
        {
        }

        // Constructor parametrizado (Usado por LINQ .Select() para crear el objeto)
        public PistaReservaDetailsDTO(int idPista, string nombrePista, int nPersonas, string nombreDeporte, decimal precio)
        {
            IdPista = idPista;
            NombrePista = nombrePista;
            NPersonas = nPersonas;
            NombreDeporte = nombreDeporte;
            Precio = precio;
        }

        public int IdPista { get; set; }

        [StringLength(50, ErrorMessage = "El nombre de la pista no puede exceder los 50 caracteres.")]
        public string NombrePista { get; set; } = "nombre por defecto";

        [Range(1, 100, ErrorMessage = "El aforo debe estar entre 1 y 100 personas.")]
        public int NPersonas { get; set; } = 1;

        // Aplanamiento: Evitamos enviar la entidad TipoDeporte entera al cliente
        [StringLength(50, ErrorMessage = "El nombre del deporte no puede exceder los 50 caracteres.")]
        public string NombreDeporte { get; set; } = "Deporte";

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El precio debe ser superior a 0.")]
        public decimal Precio { get; set; } = 1m; //La m viene de monetary, por defecto esto mostrará 1.00 si no se define valor
    }//De la clase
}//Del namespace