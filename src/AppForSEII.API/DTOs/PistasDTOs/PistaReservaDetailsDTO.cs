using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PistasDTOs
{
    public class ReservaDetailsDTO : ReservaForCreateDTO
    {
        public ReservaDetailsDTO() : base() { }

        public ReservaDetailsDTO(int id, DateTime fechaReserva, MetodoPago metodoPago, string datosPago,
                                 IList<PistaReservadaForCreateItemDTO> pistasReservadas,
                                 string clienteDNI, string clienteNombre, string clienteApellidos, 
                                 string clienteEmail, string clienteTelefono)
            // Delegamos TODA la asignación al POST
            : base(metodoPago, datosPago, pistasReservadas, clienteDNI, clienteNombre, clienteApellidos, clienteEmail, clienteTelefono, fechaReserva)
        {
            Id = id;
        }

        // DATOS EXCLUSIVOS DEL SERVIDOR (El ID)
        public int Id { get; set; }
        
        // ELIMINADA la declaración de FechaReserva para no ocultar la del padre

        public override bool Equals(object? obj)
        {
            return obj is ReservaDetailsDTO dTO &&
                   base.Equals(obj) && // El padre comprueba todo (Listas, Métodos de pago, Fecha...)
                   Id == dTO.Id;       // Yo solo compruebo mi ID
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id);
        }
    }
}