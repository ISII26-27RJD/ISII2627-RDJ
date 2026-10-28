using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PistasReservaDetailsDTOs
{
    public class PistaReservaDetailsDTO
    {
        // Constructor vacío
        public PistaReservaDetailsDTO()
        {
        }

        // Constructor parametrizado (Usado por LINQ .Select() para crear el objeto)
        public PistaReservaDetailsDTO(int idPista, string nombrePista, int nPersonas, string nombreDeporte, decimal precio)
        {
            IdPista = idPista;
            NombrePista = nombrePista;
            NPersonas = nPersonas;
            NombreDeporte = nombreDeporte;
            Precio = precio;
        }
    }//De la clase
}//Del namespace