namespace AppForSEII.API.DTOs.AlquilarMaterialDTOs
{
    
    public class ItemsMaterialDTO
    {
        
        public ItemsMaterialDTO(int MaterialID, string nombre, decimal precio, int cantidad, string tipoMaterial, string tipoDeporte)
        {
            
            this.MaterialID = MaterialID;
            Nombre = nombre;
            Precio = precio;
            Cantidad = cantidad;
            TipoMaterial = tipoMaterial;
            TipoDeporte = tipoDeporte;

        }
        
        public int MaterialID{get; set;}

        [StringLength(50, ErrorMessage = "nombre entre 3 y 50 caracteres",MinimumLength=3)]
        public string Nombre{get; set;}

        [Range(1,200,ErrorMessage ="Minimo 1")]
        public int Cantidad{get; set;}

        [Range(1,200,ErrorMessage ="Minimo 1, Maximum 200")]        
        public decimal Precio{get; set;}

        public string TipoMaterial{get; set;}

        public string TipoDeporte{get; set;}

        public override bool Equals(object? obj)
        {

            return obj is ItemsMaterialDTO dto &&
            MaterialID == dto.MaterialID &&
            Nombre == dto.Nombre &&
            Cantidad == dto.Cantidad &&
            Precio == dto.Precio &&
            TipoMaterial == dto.TipoMaterial &&
            TipoDeporte == dto.TipoDeporte;

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MaterialID, Nombre, Precio);
        }
        
    }
}