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
            // =========================================================
            // CAPA 1: Entidades Independientes (Padres)
            // =========================================================
            // CORRECCIÓN: Usamos el constructor exacto (string, string, string)
            var deporte1 = new TipoDeporte("Pádel", "Pádel", "Deporte de pala");
            var deporte2 = new TipoDeporte("Tenis", "Tenis", "Deporte de raqueta");
            
            var user = new ApplicationUser("1", "Peter", "Jackson", "peter@uclm.es");
            user.DNI = "33333333C";

            _context.Add(deporte1);
            _context.Add(deporte2);
            _context.Users.Add(user);
            
            // Primer guardado: SQLite asigna los IDs reales a los deportes (deporte1.Id, deporte2.Id)
            _context.SaveChanges(); 

            // =========================================================
            // CAPA 2: Entidades Dependientes de Nivel 1 (Hijos)
            // =========================================================
            // Usamos deporte1.Id generado por EF Core para la clave foránea
            var pista1 = new Pista("Pista Central Pádel", 4, 12m, 1, deporte1.Id);
            var pista2 = new Pista("Pista Exterior Tenis", 4, 15m, 2, deporte2.Id);

            var reservaHoy = new Reserva(user.Id, DateTime.Today, MetodoPago.Tarjeta, 27m);
            
            _context.Add(pista1);
            _context.Add(pista2);
            _context.Add(reservaHoy);
            
            // Segundo guardado: SQLite asigna IdPista a las pistas e Id a la reserva
            _context.SaveChanges(); 

            // =========================================================
            // CAPA 3: Entidades Dependientes de Nivel 2 (Nietos)
            // =========================================================
            // Usamos los IDs reales recién generados por la BD
            var pistaReservadaHoy = new PistaReservada(1, 12m, "Reserva Pádel", pista1.IdPista, reservaHoy.Id);
            var pistaReservadaHoyTenis = new PistaReservada(2, 15m, "Reserva Tenis", pista2.IdPista, reservaHoy.Id);

            _context.Add(pistaReservadaHoy);
            _context.Add(pistaReservadaHoyTenis);
            
            // Tercer y último guardado: Éxito total sin violar restricciones de Foreign Key
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
        }//del bad request

        //Fabrica de datos: Cada object?[] es una ejecución completa del test con parámetros de entrada
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

            // Parámetros inyectados: nombreDeporte, fechaReserva, listaEsperada
            var allTests = new List<object?[]>();
            allTests.Add(new object?[] { null, DateTime.Today, tc1Expected });
            allTests.Add(new object?[] { null, DateTime.Today.AddDays(1), tc2Expected });
            allTests.Add(new object?[] { "Pádel", DateTime.Today.AddDays(1), tc3Expected });

            return allTests;
        }//De la fabrica de datos

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
        }//Del ok

    }//De la clase
}//De namespace