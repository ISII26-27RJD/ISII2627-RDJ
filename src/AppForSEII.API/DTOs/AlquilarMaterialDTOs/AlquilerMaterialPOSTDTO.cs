namespace AppForSEII.API.DTOs.AlquilarMaterialDTOs
{
    
    public class AlquilerMaterialPOSTDTO
    {
        public AlquilerMaterialPOSTDTO(string DNI, string nombre,string apellidos, string telefono, Alquiler.MetodoPagoTipos metodopago, DateTime fechaAlquiuler, IList<ItemsMaterialDTO> items)
        {

            this.DNI = DNI;
            NombreUsuario = nombre;
            ApellidosUsuario = apellidos;
            NumeroTelefono = telefono;
            MetodoPago = metodopago;
            FechaAlquiler = fechaAlquiuler;
            MaterialesAlquilados = items;

        }

        public AlquilerMaterialPOSTDTO()
        {
            
            MaterialesAlquilados = new List<ItemsMaterialDTO>();

        }

        [StringLength(9, ErrorMessage = "DNI debe tener 9 caracteres",MinimumLength = 9)]
        [RegularExpression("[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][A-Za-z]")]
        public string DNI{get; set;}

        [StringLength(50, ErrorMessage = "Nombre entre 3 y 50 caracteres",MinimumLength = 3)]
        public string NombreUsuario{get; set;}

        [StringLength(50, ErrorMessage = "Nombre entre 3 y 50 caracteres",MinimumLength = 3)]
        public string ApellidosUsuario{get; set;}

        [Phone]
        public string NumeroTelefono{get; set;}

        public Alquiler.MetodoPagoTipos MetodoPago{get; set;}

        public DateTime FechaAlquiler{get; set;}

        public IList<ItemsMaterialDTO> MaterialesAlquilados{get; set;}

        public decimal PrecioTotal
        {
            get
            {
                return MaterialesAlquilados.Sum(m => m.Precio * m.Cantidad);
            }
        }

        public override bool Equals(object? obj)
        {

            return obj is AlquilerMaterialPOSTDTO dto &&
            DNI == dto.DNI &&
            NombreUsuario == dto.NombreUsuario &&
            ApellidosUsuario == dto.ApellidosUsuario &&
            NumeroTelefono == dto.NumeroTelefono &&
            MetodoPago == dto.MetodoPago &&
            FechaAlquiler == dto.FechaAlquiler &&
            MaterialesAlquilados.SequenceEqual(dto.MaterialesAlquilados);

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(),FechaAlquiler, NombreUsuario);
        }

    }
}