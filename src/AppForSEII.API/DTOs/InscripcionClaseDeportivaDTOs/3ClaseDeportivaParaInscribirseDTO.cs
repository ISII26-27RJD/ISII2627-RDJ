namespace AppForSEII.Shared.ClassDTOs
{
    // DTO de respuesta del GET (catalogo). Contiene los datos de una clase deportiva
    // que el usuario puede ver antes de decidir inscribirse.
    // Solo expone los campos necesarios para el listado — no toda la entidad ClaseDeportiva.
    public class ClaseDeportivaParaInscribirseDTO
    {
        // Constructor que inicializa todos los campos del DTO a partir de los datos de la entidad.
        public ClaseDeportivaParaInscribirseDTO(int id, string descripcion, string tipoDeporte, string monitor, string nivel, DateTime fechaHora, string? lugar, int plazasDisponibles, decimal precioUnitario)
        {
            Id = id;
            Descripcion = descripcion;
            TipoDeporte = tipoDeporte;
            Monitor = monitor;
            Nivel = nivel;
            FechaHora = fechaHora;
            Lugar = lugar;
            PlazasDisponibles = plazasDisponibles;
            PrecioUnitario = precioUnitario;
        }

        // Identificador unico de la clase deportiva.
        public int Id { get; set; }
        // Descripcion o nombre de la clase (p.ej. "Clase introductoria de padel para principiantes.").
        public string Descripcion { get; set; }
        // Nombre del tipo de deporte al que pertenece la clase (p.ej. "Padel", "Tenis").
        public string TipoDeporte { get; set; }
        // Nombre del monitor que imparte la clase.
        public string Monitor { get; set; }
        // Nivel de dificultad de la clase (Principiante, Intermedio, Avanzado...).
        public string Nivel { get; set; }
        // Fecha y hora de celebracion de la clase.
        public DateTime FechaHora { get; set; }
        // Lugar donde se imparte la clase (puede ser null si no esta especificado).
        public string? Lugar { get; set; }
        // Numero de plazas disponibles restantes. El GET solo devuelve clases con plazas > 0.
        public int PlazasDisponibles { get; set; }
        // Precio por plaza. El precio total dependera de las plazas reservadas.
        public decimal PrecioUnitario { get; set; }

        // Equals por valor para poder usar Assert.Equal en los tests unitarios.
        // Compara todos los campos uno a uno.
        public override bool Equals(object? obj)
        {
            return obj is ClaseDeportivaParaInscribirseDTO other &&
                   Id == other.Id &&
                   Descripcion == other.Descripcion &&
                   TipoDeporte == other.TipoDeporte &&
                   Monitor == other.Monitor &&
                   Nivel == other.Nivel &&
                   FechaHora == other.FechaHora &&
                   Lugar == other.Lugar &&
                   PlazasDisponibles == other.PlazasDisponibles &&
                   PrecioUnitario == other.PrecioUnitario;
        }

        // GetHashCode que incluye todos los campos del DTO.
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Descripcion);
            hash.Add(TipoDeporte);
            hash.Add(Monitor);
            hash.Add(Nivel);
            hash.Add(FechaHora);
            hash.Add(Lugar);
            hash.Add(PlazasDisponibles);
            hash.Add(PrecioUnitario);
            return hash.ToHashCode();
        }
    }
}
