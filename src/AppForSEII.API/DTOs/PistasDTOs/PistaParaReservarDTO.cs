using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PistasDTOs
{
    public class PistaParaReservarDTO
    {
        // Para serializar necesitamos un constructor vacio
        public PistaParaReservarDTO()
        {
        }

        // Constructor parametrizado
        public PistaParaReservarDTO(int idPista, string nombrePista, int nPersonas, string nombreDeporte, decimal precio)
        {
            IdPista = idPista;
            NombrePista = nombrePista;
            NPersonas = nPersonas;
            NombreDeporte = nombreDeporte;
            Precio = precio;
        }

        public int IdPista{get; set;}

        [StringLength(50, ErrorMessage = "El nombre de la pista no puede exceder los 50 caracteres.")]
        public string NombrePista {get; set;} = "nombre por defecto";

        [Range(1, 100, ErrorMessage = "El aforo debe estar entre 1 y 100 personas.")]
        public int NPersonas { get; set; } = 1;

        //Aquí es donde introducimos el "aplanado", no enviamos un objeto "TipoDeporte", solo nos interesa el nonbre del deporte
        //NOTA PARA RECORDAR: en paso 2 nos piden el nombre del tipo de deporte de la pista a ser alquilada.
        [StringLength(50, ErrorMessage = "El nombre del deporte no puede exceder los 50 caracteres.")]
        public string NombreDeporte { get; set; } = "Deporte";

        [Required] // Esto es por la referencia que tenemos a mano. Entiendo que una reserva de una pista nunca debe ser "accidentalmente gratis"
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El precio debe ser superior a 0.")]
        public decimal Precio { get; set; } = 1m;

        // comparar objetos en los Tests
        public override bool Equals(object? obj)
        {
            return obj is PistaParaReservarDTO dTO &&
                   IdPista == dTO.IdPista &&
                   NombrePista == dTO.NombrePista &&
                   NPersonas == dTO.NPersonas &&
                   NombreDeporte == dTO.NombreDeporte &&
                   Precio == dTO.Precio;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IdPista, NombrePista, NPersonas, NombreDeporte, Precio);
        }
    }
}