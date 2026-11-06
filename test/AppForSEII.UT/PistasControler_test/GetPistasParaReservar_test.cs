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
    public class GetPistasParaReservar_test : AppForSEII4SqliteUT
    {
        public GetPistasParaReservar_test()
        {
            // Instancias creadas EXCLUSIVAMENTE con constructores
            var deporte1 = new TipoDeporte(1, "Pádel", "Deporte de pala");
            var deporte2 = new TipoDeporte(2, "Tenis", "Deporte de raqueta");
            
            var deportes = new List<TipoDeporte>();
            deportes.Add(deporte1);
            deportes.Add(deporte2);

            // En las pistas ponemos sus IDs (1 y 2)
            var pista1 = new Pista(1, "Pista Central Pádel", 4, 12m, 1, 1);
            var pista2 = new Pista(2, "Pista Exterior Tenis", 4, 15m, 2, 2);

            var pistas = new List<Pista>();
            pistas.Add(pista1);
            pistas.Add(pista2);

            var user = new ApplicationUser("1", "Peter", "Jackson", "peter@uclm.es");
            user.DNI = "33333333C";

            // Agotamos stock de la pista de Pádel para el día de HOY
            var reservaHoy = new Reserva(1, user.Id, DateTime.Today, MetodoPago.Tarjeta, 12m);
            
            
            var pistaReservadaHoy = new PistaReservada(1, 12m, "Reserva", 1, reservaHoy.Id);
            pistaReservadaHoy.Pista = pista1;
            reservaHoy.PistasReservadas.Add(pistaReservadaHoy);

            // Agotamos TOTALMENTE el stock de la pista de Tenis (Stock = 2 -> Cantidad = 2)
            var pistaReservadaHoyTenis = new PistaReservada(2, 15m, "Reserva Tenis", 2, reservaHoy.Id);
            pistaReservadaHoyTenis.Pista = pista2;
            reservaHoy.PistasReservadas.Add(pistaReservadaHoyTenis);

            // Inyectamos a la BD falsa de SQLite
            _context.AddRange(deportes);
            _context.AddRange(pistas);
            _context.Users.Add(user);
            _context.Reservas.Add(reservaHoy);
            _context.SaveChanges();
        }//Constructor

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetPistasParaReservar_BadRequest_FechasPasadas_test()
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);
            DateTime fechaPasada = DateTime.Today.AddDays(-1);

            // Act
            var result = await controller.GetPistasParaReservar(null, fechaPasada);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var error = problemDetails.Errors.First().Value[0];
            Assert.Equal("La fecha de reserva no puede ser anterior al día de hoy.", error);
        }

        
    }//De la clase
}//De namespace