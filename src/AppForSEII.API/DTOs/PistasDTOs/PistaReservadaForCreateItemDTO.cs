using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.ReservaDTOs
{
    public class PistaReservadaForCreateItemDTO
    {
        [Required(ErrorMessage = "Debe indicar el ID de la pista que desea reservar.")]
        public int IdPista { get; set; }

        // El Caso de Uso dice explícitamente: "Como campos opcionales [...] observaciones"
        [StringLength(400, ErrorMessage = "Las observaciones no pueden exceder los 400 caracteres.")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string? Observaciones { get; set; }

        public PistaReservadaForCreateItemDTO() { }

        public PistaReservadaForCreateItemDTO(int idPista, string? observaciones = null)
        {
            IdPista = idPista;
            Observaciones = observaciones;
        }

        // Sobrescribimos Equals para facilitar los Unit Tests en el futuro
        public override bool Equals(object? obj)
        {
            return obj is PistaReservadaForCreateItemDTO dTO &&
                   IdPista == dTO.IdPista &&
                   Observaciones == dTO.Observaciones;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IdPista, Observaciones);
        }
    }
}