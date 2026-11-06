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
            // 1. Arrange: Preparación de datos usando constructores sin ID manual (Fidelidad al material)
            var deporte1 = new TipoDeporte("Pádel", "Deporte de pala", "Similar al tenis");
            var deporte2 = new TipoDeporte("Tenis", "Deporte de raqueta", "Bueno para principiantes");
            
            var deportes = new List<TipoDeporte>();
            deportes.Add(deporte1);
            deportes.Add(deporte2);

            // Pista 1 (Pádel): Stock 1. Pista 2 (Tenis): Stock 2. (IDs generados por EF)
            var pista1 = new Pista("Pista Central Pádel", 4, 12m, 1, 1);
            var pista2 = new Pista("Pista Exterior Tenis", 4, 15m, 2, 2);

            var pistas = new List<Pista>();
            pistas.Add(pista1);
            pistas.Add(pista2);

            var user = new ApplicationUser("1", "Peter", "Jackson", "peter@uclm.es");
            user.DNI = "33333333C";

            // Creamos la reserva para HOY sin forzar el ID
            var reservaHoy = new Reserva(user.Id, DateTime.Today, MetodoPago.Tarjeta, 27m);
            
            // Agotamos el stock asociando los objetos directamente (EF Core resolverá las FKs)
            var pistaReservadaHoy = new PistaReservada(1, 12m, "Reserva Pádel", 0, 0); // IDs foráneos a 0, se mapean por el objeto
            pistaReservadaHoy.Pista = pista1;
            pistaReservadaHoy.Reserva = reservaHoy;
            reservaHoy.PistasReservadas.Add(pistaReservadaHoy);

            var pistaReservadaHoyTenis = new PistaReservada(2, 15m, "Reserva Tenis", 0, 0);
            pistaReservadaHoyTenis.Pista = pista2;
            pistaReservadaHoyTenis.Reserva = reservaHoy;
            reservaHoy.PistasReservadas.Add(pistaReservadaHoyTenis);

            // Guardamos en la BD de pruebas. EF Core asigna los IDs autoincrementales aquí.
            _context.AddRange(deportes);
            _context.AddRange(pistas);
            _context.Users.Add(user);
            _context.Reservas.Add(reservaHoy);
            _context.SaveChanges();
        }

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

        public static IEnumerable<object?[]> TestCasesFor_GetPistas_OK()
        {
            // Los DTOs SÍ requieren el ID explícito en el constructor, tal y como se ve en MovieForRentalDTO
            var pistaPadelDTO = new PistaParaReservarDTO(1, "Pista Central Pádel", 4, "Pádel", 12m);
            var pistaTenisDTO = new PistaParaReservarDTO(2, "Pista Exterior Tenis", 4, "Tenis", 15m);

            // TC1: Hoy, sin filtro -> Ambas agotadas, lista vacía
            var tc1Expected = new List<PistaParaReservarDTO>();

            // TC2: Mañana, sin filtro -> Salen ambas
            var tc2Expected = new List<PistaParaReservarDTO>();
            tc2Expected.Add(pistaPadelDTO);
            tc2Expected.Add(pistaTenisDTO);
            tc2Expected = tc2Expected.OrderBy(p => p.NombrePista).ToList();

            // TC3: Mañana, filtro "Pádel" -> Sale solo Pádel
            var tc3Expected = new List<PistaParaReservarDTO>();
            tc3Expected.Add(pistaPadelDTO);

            var allTests = new List<object?[]>();
            allTests.Add(new object?[] { null, DateTime.Today, tc1Expected });
            allTests.Add(new object?[] { null, DateTime.Today.AddDays(1), tc2Expected });
            allTests.Add(new object?[] { "Pádel", DateTime.Today.AddDays(1), tc3Expected });

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetPistas_OK))]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetPistasParaReservar_OK_test(string? deporte, DateTime? fecha, List<PistaParaReservarDTO> expected)
        {
            // Arrange
            var mock = new Mock<ILogger<PistasController>>();
            var controller = new PistasController(_context, mock.Object);

            // Act
            var result = await controller.GetPistasParaReservar(deporte, fecha);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualList = Assert.IsType<List<PistaParaReservarDTO>>(okResult.Value);
            
            // Verificación exigida usando Equals() implícito
            Assert.Equal(expected, actualList); 
        }

    }//De la clase
}//De namespace