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
    public class CreatePistaReserva_test : AppForSEII4SqliteUT
    {
        public CreatePistaReserva_test()
        {
            // --- ARRANGE COMPARTIDO: Base de datos mínima ---
            var deporte = new TipoDeporte("Pádel", "Pádel", "Deporte de pala");
            
            var user = new ApplicationUser("1", "Peter", "Jackson", "peter@uclm.es");
            user.DNI = "33333333C";
            user.PhoneNumber = "666555444";

            // Pista ID 1: Stock 1, Precio 12.
            var pista = new Pista("Pista Central Pádel", 4, 12m, 1, 0);
            pista.TipoDeporte = deporte; 

            // Reserva PREVIA para simular que HOY ya está ocupada
            var reservaPrevia = new Reserva(user.Id, DateTime.Today, MetodoPago.Tarjeta, 12m);
            reservaPrevia.Cliente = user; 

            var pistaReservada = new PistaReservada(1, 12m, "Ya ocupada", 0, 0);
            pistaReservada.Pista = pista;
            pistaReservada.Reserva = reservaPrevia;
            reservaPrevia.PistasReservadas.Add(pistaReservada);

            _context.Add(deporte);
            _context.Add(pista);
            _context.Users.Add(user);
            _context.Add(reservaPrevia);
            _context.SaveChanges(); 
            // En este punto, PistaCentral (Id 1) tiene Stock=1, y ya tiene 1 ocupada HOY.
        }//Del constructor
        
    }//De la clase
}//Del namespace