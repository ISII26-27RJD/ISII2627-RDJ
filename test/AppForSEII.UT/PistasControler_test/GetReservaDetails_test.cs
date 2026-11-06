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
        }//Del constructor

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetReservaDetails_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);

            // Act: Buscamos un ID que no existe (0)
            var result = await controller.GetReservaDetails(0);

            // Assert: Verificamos que devuelve el código 404 (NotFound)
            Assert.IsType<NotFoundResult>(result);
        }//De not found

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetReservaDetails_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);

            var pistaDto = new PistaReservadaForCreateItemDTO(1, "Sin observaciones", "Pista Central Pádel", "Pádel", 4, 12m);
            var pistasDtoList = new List<PistaReservadaForCreateItemDTO>();
            pistasDtoList.Add(pistaDto);

            // Creamos el ticket esperado exacto. 
            var expectedDetails = new ReservaDetailsDTO(
                1, 
                DateTime.Today, 
                MetodoPago.Tarjeta, 
                "****-****-****-****", 
                pistasDtoList, 
                "33333333C", 
                "Peter", 
                "Jackson", 
                "peter@uclm.es", 
                "666555444"
            );

            // Act: Buscamos el ID 1, que es el que SQLite generó en el constructor
            var result = await controller.GetReservaDetails(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualDetails = Assert.IsType<ReservaDetailsDTO>(okResult.Value);
            
            //  Tenemos que usar Assert.Equal para invocar el override Equals
            Assert.Equal(expectedDetails, actualDetails);
        }//De found
        
    }//De la clase
}//Del namespace