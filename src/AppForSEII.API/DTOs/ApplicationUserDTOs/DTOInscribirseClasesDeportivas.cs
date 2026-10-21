namespace AppForSEII.Shared.ClassDTOs
{
    public class DTOInscribirseClasesDeportivas
    {
        public DTOInscribirseClasesDeportivas(int id, string descripcion, string tipoDeporte, string monitor, string nivel, DateTime fechaHora, string? lugar, int plazasDisponibles, decimal precioUnitario)
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

        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string TipoDeporte { get; set; }
        public string Monitor { get; set; }
        public string Nivel { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Lugar { get; set; }
        public int PlazasDisponibles { get; set; }
        public decimal PrecioUnitario { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is DTOInscribirseClasesDeportivas other &&
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