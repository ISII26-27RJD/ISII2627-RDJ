using System.ComponentModel.DataAnnotations;
using System.Linq;
using AppForSEII.API.Models;


namespace AppForSEII.API.DTOs.PistasDTOs
{
    public class ReservaForCreateDTO
    {
        public ReservaForCreateDTO()
        {
            PistasReservadas = new List<PistaReservadaForCreateItemDTO>();
        }

        public ReservaForCreateDTO(MetodoPago metodoPago, string datosPago, IList<PistaReservadaForCreateItemDTO> pistasReservadas, 
                                   string clienteDNI = "", string clienteNombre = "", string clienteApellidos = "", 
                                   string clienteEmail = "", string clienteTelefono = "")
        {
            PaymentMethod = metodoPago; //No pongo MetodoPago porque el modelo tiene ese mismo nombre
            DatosPago = datosPago ?? throw new ArgumentNullException(nameof(datosPago));
            PistasReservadas = pistasReservadas ?? new List<PistaReservadaForCreateItemDTO>();
            
            ClienteDNI = clienteDNI;
            ClienteNombre = clienteNombre;
            ClienteApellidos = clienteApellidos;
            ClienteEmail = clienteEmail;
            ClienteTelefono = clienteTelefono;
        }

        //DATOS DE ENTRADA
        //Leyendo el enunciado, nos piden que el cliente introduzca un método de pago, y datos del pago.
        //Además, esto no lo pone en el pdf, pero se puede entender que si el cliente quiere hacer una reserva, tiene que dejar una lista de las pistas a reservar.
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public MetodoPago PaymentMethod { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Los datos del pago son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los datos de pago no pueden exceder los 100 caracteres.")]
        public string DatosPago { get; set; } = string.Empty;
        
        // Propiedad opcional para elegir cuándo se quiere la pista
        public DateTime? FechaReserva { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos una pista en la reserva.")]
        [MinLength(1, ErrorMessage = "La reserva debe contener al menos una pista.")]
        public IList<PistaReservadaForCreateItemDTO> PistasReservadas { get; set; }

        //DATOS DE SALIDA
        //Además de lo que se muestra de cada pista (nombre de la pista, tipo de deporte, precio y nº personas), tenemos que recuperar datos del cliente
        //Esto los rellena el propio servidor, pero también serán usados por el DETAILS
        public string ClienteDNI { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellidos { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;

        //LOGICA DE NEGOCIO
        //Al observar el ejemplo que tengo dentro de AppForMovies, al utilizar LINQ (.Sum), voy a asegurar que el Flujo 3 se respete:
        //"Si modifico el carrito para borrar las pistas que no me interesan, el sistema actualiza el precio total del carrito"

        public decimal PrecioTotal
        {
            get
            {
                return PistasReservadas.Sum(pr => pr.Precio);
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is ReservaForCreateDTO dTO &&
                   PaymentMethod == dTO.PaymentMethod &&
                   DatosPago == dTO.DatosPago &&
                   ClienteDNI == dTO.ClienteDNI &&
                   ClienteNombre == dTO.ClienteNombre &&
                   ClienteApellidos == dTO.ClienteApellidos &&
                   ClienteEmail == dTO.ClienteEmail &&
                   ClienteTelefono == dTO.ClienteTelefono &&
                   PrecioTotal == dTO.PrecioTotal &&
                   PistasReservadas.SequenceEqual(dTO.PistasReservadas);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PaymentMethod, DatosPago, ClienteDNI, ClienteNombre, PrecioTotal);
        }

        //No ponemos id del cliente porque el frontend los muestra en pantalla, pero no debe enviárnoslos de vuelta en el POST

        //Por cierto, no metas aquí constructor vacio, ya lo tienen los items
        
    }//De la clase
}//Del namespace