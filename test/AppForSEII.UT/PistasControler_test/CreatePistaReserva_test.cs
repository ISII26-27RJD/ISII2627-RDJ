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

            var pistaReservada = new PistaReservada(1, 12m, "Ya ocupada", 1, 0);
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

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreatePistaReserva_InvalidModelState_test()
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);
            // Simulamos que el framework ha detectado un error (ej. lista de pistas vacía)
            controller.ModelState.AddModelError("PistasReservadas", "Required");

            var badDto = new ReservaForCreateDTO(); 

            // Act
            var result = await controller.CreatePistaReserva(badDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreatePistaReserva_Unauthorized_UserNotFound_test()
        {
            // Arrange
            // Vaciamos la tabla de usuarios en este test para simular que Peter no existe
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);

            var dto = new ReservaForCreateDTO(
                MetodoPago.Tarjeta,
                "1234",
                new List<PistaReservadaForCreateItemDTO>());
            
            // Act
            var result = await controller.CreatePistaReserva(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Usuario no autenticado.", unauthorizedResult.Value);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreatePistaReserva_BadRequest_PistaNoEncontrada_test()
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);

            var itemDto = new PistaReservadaForCreateItemDTO(99, "Inventada"); // Pista 99 no existe
            var list = new List<PistaReservadaForCreateItemDTO> { itemDto };
            var dto = new ReservaForCreateDTO(MetodoPago.Tarjeta, "1234", list);

            // Act
            var result = await controller.CreatePistaReserva(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Contains("La pista con ID 99 no existe.", problemDetails.Errors["PistaNoEncontrada"][0]);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreatePistaReserva_BadRequest_SinStock_test()
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);

            // Pedimos la Pista 1 para HOY. 
            // Como en el constructor metimos una reserva previa para hoy, el stock (1) se superará.
            var itemDto = new PistaReservadaForCreateItemDTO(1, "Quiero reservar sobre límite");
            var list = new List<PistaReservadaForCreateItemDTO> { itemDto };
            
            var dto = new ReservaForCreateDTO(MetodoPago.Tarjeta, "1234", list);
            dto.FechaReserva = DateTime.Today; 

            // Act
            var result = await controller.CreatePistaReserva(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Contains("ya está agotada", problemDetails.Errors["SinStock"][0]);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreatePistaReserva_Created_OK_test()
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);

            // Pedimos la Pista 1 para MAÑANA. (Mañana no hay reservas en BD, así que hay stock).
            var itemDto = new PistaReservadaForCreateItemDTO(1, "Reservar mañana");
            var list = new List<PistaReservadaForCreateItemDTO> { itemDto };
            
            var postDto = new ReservaForCreateDTO(MetodoPago.Tarjeta, "1234567812345678", list);
            postDto.FechaReserva = DateTime.Today.AddDays(1); 

            // Creamos el ticket esperado para comparar con el Assert.Equal
            var expectedItem = new PistaReservadaForCreateItemDTO(1, "Reservar mañana", "Pista Central Pádel", "Pádel", 4, 12m);
            var expectedList = new List<PistaReservadaForCreateItemDTO> { expectedItem };
            
            var expectedDetails = new ReservaDetailsDTO(
                2, // La reserva previa era la 1. Esta nueva será la ID 2.
                DateTime.Today.AddDays(1), 
                MetodoPago.Tarjeta, 
                "****-****-****-****", 
                expectedList, 
                "33333333C", 
                "Peter", 
                "Jackson", 
                "peter@uclm.es", 
                "666555444" 
            );

            // Act
            var result = await controller.CreatePistaReserva(postDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetReservaDetails", createdResult.ActionName); // Tu controlador devuelve esto
            
            var actualDetails = Assert.IsType<ReservaDetailsDTO>(createdResult.Value);
            Assert.Equal(expectedDetails, actualDetails);
        }
    }//De la clase
}//Del namespace