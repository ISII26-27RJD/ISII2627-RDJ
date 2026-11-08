using System.ComponentModel.DataAnnotations;

namespace AppForSEII.Shared.ClassDTOs
{
    // DTO que recibe el API en el body del POST para crear una nueva inscripcion a clase deportiva.
    // Contiene los datos del usuario inscrito, el metodo de pago y la lista de clases a las que se apunta.
    public class ClaseDeportivaInscripcionForCreateDTO
    {
        // Nombre de usuario (email) del cliente. Obligatorio — el controlador lo usa para buscar al usuario en la BD.
        [Required]
        public string NombreUsuario { get; set; }

        // Apellidos del cliente. Obligatorio.
        [Required]
        public string ApellidosUsuario { get; set; }

        // DNI del cliente. Obligatorio — tambien se usa como segundo criterio de busqueda del usuario.
        [Required]
        public string Dni { get; set; }

        // Telefono de contacto del cliente. Obligatorio.
        [Required]
        public string Telefono { get; set; }

        // Datos del pago (referencia, numero de tarjeta enmascarado, etc.). Obligatorio.
        [Required]
        public string DatosPago { get; set; }

        // Metodo de pago elegido (Bizum, Tarjeta, etc.). Obligatorio.
        [Required]
        public MetodoPagoTypes MetodoPago { get; set; }

        // Lista de clases deportivas a las que se quiere inscribir, con las plazas y observaciones de cada una.
        // Se inicializa como lista vacia para evitar NullReferenceException si el cliente no la envia.
        public IList<ClassInscriptionCreateItemDTO> ClasesInscritas { get; set; } = new List<ClassInscriptionCreateItemDTO>();
    }

    // DTO que representa cada clase deportiva dentro del body del POST (una linea del pedido de inscripcion).
    public class ClassInscriptionCreateItemDTO
    {
        // ID de la clase deportiva a la que el usuario quiere inscribirse. Obligatorio.
        [Required]
        public int ClaseDeportivaId { get; set; }

        // Numero de plazas que el usuario quiere reservar. Minimo 1, obligatorio.
        [Required]
        [Range(1, int.MaxValue)]
        public int PlazasReservadas { get; set; }

        // Observaciones opcionales del alumno para esta clase concreta (alergias, lesiones, etc.).
        public string? Observaciones { get; set; }
    }
}
