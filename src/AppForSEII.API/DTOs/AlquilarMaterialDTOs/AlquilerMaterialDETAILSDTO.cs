using Humanizer;

namespace AppForSEII.API.DTOs.AlquilarMaterialDTOs
{

    public class AlquilerMaterialDETAILSDTO : AlquilerMaterialPOSTDTO
    {
        public AlquilerMaterialDETAILSDTO()
        {
        }

        public AlquilerMaterialDETAILSDTO(int ID, string DNI, string nombre, string apellidos, string telefono, Alquiler.MetodoPagoTipos metodopago, DateTime fechaAlquiuler, IList<ItemsMaterialDTO> items) 
        : base(DNI, nombre, apellidos, telefono, metodopago, fechaAlquiuler, items)
        {
            this.ID = ID;
        }

        public int ID{get; set;}

        public override bool Equals(object? obj)
        {
            return obj is AlquilerMaterialDETAILSDTO dto &&
            ID == dto.ID &&
            base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ID);
        }
    }
}