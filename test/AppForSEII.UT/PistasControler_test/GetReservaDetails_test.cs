using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PistasDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppForSEII.UT.PistasController_test
{
    public class GetReservaDetails_test : AppForSEII4SqliteUT
    {
        //CONSTRUCTOR
        public GetReservaDetails_test()
        {
            // Instanciamos los objetos del padre
            var deporte = new TipoDeporte("Pádel", "Pádel", "Deporte de pala");
            
            var user = new ApplicationUser("1", "Peter", "Jackson", "peter@uclm.es");
            user.DNI = "33333333C";
            user.PhoneNumber = "666555444"; // Añadido para el DTO

            // Instanciamos Pista y enlazamos por referencia explicita
            var pista = new Pista("Pista Central Pádel", 4, 12m, 2, 0);
            pista.TipoDeporte = deporte; 

            // Instanciamos Reserva y enlazamos
            var reserva = new Reserva(user.Id, DateTime.Today, MetodoPago.Tarjeta, 12m);
            reserva.Cliente = user; 

            // Instanciamos PistaReservada y cruzamos las referencias
            var pistaReservada = new PistaReservada(1, 12m, "Sin observaciones", 0, 0);
            pistaReservada.Pista = pista;
            pistaReservada.Reserva = reserva;
            reserva.PistasReservadas.Add(pistaReservada);

            // Inyectamos en SQLite y generamos los IDs reales
            _context.Add(deporte);
            _context.Add(pista);
            _context.Users.Add(user);
            _context.Add(reserva);
            _context.SaveChanges();
        }
    }//De la clase
}//Del namespace