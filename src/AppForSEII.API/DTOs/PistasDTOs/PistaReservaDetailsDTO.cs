using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PistasDTOs
{
    // Los detalles no es concepto aislado; es exactamente la misma petición 
    // original de POST, pero enriquecida con los sellos oficiales del servidor (ID y Fecha).
    public class ReservaDetailsDTO : ReservaForCreateDTO
    {
        public ReservaDetailsDTO() : base() { }

        // CORRECCION: added 'string datosPago' para que el constructor base no colapse.
        public ReservaDetailsDTO(int id, DateTime fechaReserva, MetodoPago metodoPago, string datosPago,
                                 IList<PistaReservadaForCreateItemDTO> pistasReservadas,
                                 string clienteDNI, string clienteNombre, string clienteApellidos, 
                                 string clienteEmail, string clienteTelefono)
            // Delegamos toda la asignación de datos a la clase padre, el POST
            // Esto evita violar el principio DRY (Don't Repeat Yourself).
            : base(metodoPago, datosPago, pistasReservadas, clienteDNI, clienteNombre, clienteApellidos, clienteEmail, clienteTelefono)
        {
            Id = id;
            FechaReserva = fechaReserva;
        }

        // DATOS EXCLUSIVOS DEL SERVIDOR (No existen en el POST del cliente)
        public int Id { get; set; }
        public DateTime FechaReserva { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ReservaDetailsDTO dTO &&
                   //Si no uso base.Equals(obj, sufro el riesgo de ver falsos positivos, ya que solo comparamos Id y fecha,
                   //y no miramos si las pistas o el método de pago son los mismos
                   base.Equals(obj) &&
                   Id == dTO.Id &&

                   //Es necesario el uso de .Date porque c# captura milisegundos. Se puede oler que en los milisegundos no seremos iguales,
                   //por lo que pueden fallar la comparación de la fecha en la base de datos con la de la vida real.
                   FechaReserva.Date == dTO.FechaReserva.Date;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaReserva);
        }
    }
}