using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PistasDTOs
{
    public class PistaReservadaForCreateItemDTO
    {
        public PistaReservadaForCreateItemDTO() { }
        public PistaReservadaForCreateItemDTO(int idPista, string? observaciones, string nombrePista = "", string tipoDeporte = "", int nPersonas = 0, decimal precio = 0)
        {
            IdPista = idPista;
            Observaciones = observaciones;
            NombrePista = nombrePista;
            TipoDeporte = tipoDeporte;
            NPersonas = nPersonas;
            Precio = precio;
        }

        // IdPista y Observaciones son los datos de entrada que el usuario puede meter "en cada pista". 
        // Observaciones es opcional, por eso la interrogación.
        [Required(ErrorMessage = "Debe indicar el ID de la pista que desea reservar.")]
        public int IdPista { get; set; }

        [StringLength(400, ErrorMessage = "Las observaciones no pueden exceder los 400 caracteres.")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string? Observaciones { get; set; }

        //Datos de salida.
        //He replicado la estructura que tengo dentro de RentalItemDTO.
        //Estos atributos los usará el DTO de detalles, para que se muestren al finalizar la reserva.
        public string NombrePista { get; set; } = string.Empty;
        public string TipoDeporte { get; set; } = string.Empty;
        public int NPersonas { get; set; }
        public decimal Precio { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is PistaReservadaForCreateItemDTO dTO &&
                   IdPista == dTO.IdPista &&
                   Observaciones == dTO.Observaciones &&
                   NombrePista == dTO.NombrePista &&
                   TipoDeporte == dTO.TipoDeporte &&
                   NPersonas == dTO.NPersonas &&
                   Precio == dTO.Precio;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IdPista, Observaciones, NombrePista, TipoDeporte, NPersonas, Precio);
        }

    }//De la clase

}//Del namespace