namespace AppForSEII.API.DTOs.AlquilarMaterialDTOs
{
    
    public class MaterialAlquilarDTO
    {

        public MaterialAlquilarDTO()
        {
        }

        public MaterialAlquilarDTO(int id, string nombre, decimal precio, string tipoMaterial, string tipoDeporte)
        {
            this.id = id;
            Nombre = nombre;
            Precio = precio;
            TipoMaterial = tipoMaterial;
            TipoDeporte = tipoDeporte;
        }

        public int id;

        [Range(0,200,ErrorMessage ="Minimo 0, Maximum 200")]
        public int Cantidad{get; set;}

        [StringLength(50, ErrorMessage = "nombre entre 3 y 50 caracteres",MinimumLength=3)]
        public string Nombre{get; set;}

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(1, float.MaxValue, ErrorMessage = "precio minimo 1")]
        public decimal Precio
        {
            get; set;
        }=1;

        [StringLength(50, ErrorMessage = "nombre entre 3 y 50 caracteres",MinimumLength=3)]
        public string TipoMaterial{get; set;}

        [StringLength(50, ErrorMessage = "nombre entre 3 y 50 caracteres",MinimumLength=3)]
        public string TipoDeporte{get; set;}

        public override bool Equals(object? obj)
        {
            return obj is MaterialAlquilarDTO dTO &&
            id == dTO.id &&
            Cantidad == dTO.Cantidad &&
            Nombre == dTO.Nombre &&
            Precio == dTO.Precio &&
            TipoMaterial == dTO.TipoMaterial &&
            TipoDeporte == dTO.TipoDeporte;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, Nombre, Cantidad, Precio, TipoMaterial, TipoDeporte);
        }

    }
}
