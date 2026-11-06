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

        //Fabrica de datos: Cada object?[] es una ejecución completa del test con parámetros de entrada
        public static IEnumerable<object?[]> TestCasesFor_GetPistas_OK()
        {
            // Exigencia estricta: Solo usar constructores parametrizados
            var pistaPadelDTO = new PistaParaReservarDTO(1, "Pista Central Pádel", 4, "Pádel", 12m);
            var pistaTenisDTO = new PistaParaReservarDTO(2, "Pista Exterior Tenis", 4, "Tenis", 15m);

            // TC1: Hoy, sin filtro de deporte -> Ambas están agotadas, la lista debe estar VACÍA
            var tc1Expected = new List<PistaParaReservarDTO>();

            // TC2: Mañana, sin filtro -> Salen ambas (mañana no hay reservas), ordenadas por nombre
            var tc2Expected = new List<PistaParaReservarDTO>();
            tc2Expected.Add(pistaPadelDTO);
            tc2Expected.Add(pistaTenisDTO);
            tc2Expected = tc2Expected.OrderBy(p => p.NombrePista).ToList();

            // TC3: Mañana, filtrando por "Pádel" -> Sale solo Pádel
            var tc3Expected = new List<PistaParaReservarDTO>();
            tc3Expected.Add(pistaPadelDTO);

            var allTests = new List<object?[]>();
            // Parámetros inyectados: nombreDeporte, fechaReserva, listaEsperada
            allTests.Add(new object?[] { null, DateTime.Today, tc1Expected });
            allTests.Add(new object?[] { null, DateTime.Today.AddDays(1), tc2Expected });
            allTests.Add(new object?[] { "Pádel", DateTime.Today.AddDays(1), tc3Expected });

            return allTests;
        }//De la fábrica de datos

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
            
            // Regla de penalización: Obligatorio usar Assert.Equal para invocar el override Equals
            Assert.Equal(expected, actualList); 
        }//Del ok

    }//De la clase
}//De namespace