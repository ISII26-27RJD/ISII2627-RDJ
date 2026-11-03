using System.ComponentModel.DataAnnotations;
using AppForSEII.API.Models;


namespace AppForSEII.API.DTOs.ReservaDTOs
{
    public class ReservaForCreateDTO
    {
        // El caso de uso dice que solicitemos un método de pago [...]
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public MetodoPago MetodoPago { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos una pista en la reserva.")]
        [MinLength(1, ErrorMessage = "La reserva debe contener al menos una pista.")]
        public IList<PistaReservadaForCreateItemDTO> PistasReservadas { get; set; }
    }//De la clase
}//Del namespace