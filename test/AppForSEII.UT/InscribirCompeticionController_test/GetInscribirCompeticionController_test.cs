using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppForSEII.API.Controllers;
using AppForSEII.API.Models;
using AppForSEII.Shared.CompetitionDTOs;
using Moq;

namespace AppForSEII.UT.GetInscribirCompeticionController_test
{
    public class GetInscribirCompeticionController_test : AppForSEII4SqliteUT
    {
        private readonly ITestOutputHelper _output;

        public GetInscribirCompeticionController_test(ITestOutputHelper output)
        {
            _output = output;

            var tipoDeporte1 = new TipoDeporte("Running", "Carrera a pie", "Pruebas de running");
            var tipoDeporte2 = new TipoDeporte("Ciclismo", "Ciclismo en ruta", "Pruebas de bicicleta");

            _context.AddRange(tipoDeporte1);
            _context.AddRange(tipoDeporte2);
            _context.SaveChanges();

            var comp1 = new Competicion("Parque", new DateTime(2026, 6, 15), 5, 20.0m, tipoDeporte1.Id, "Albacete");
            comp1.CompeticionesInscritas = new List<CompeticionInscrita>();

            var comp2 = new Competicion("Circuito", new DateTime(2026, 7, 20), 2, 50.0m, tipoDeporte2.Id, "Toledo");
            comp2.CompeticionesInscritas = new List<CompeticionInscrita>();

            _context.AddRange(comp1, comp2);
            _context.SaveChanges();
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetCompetitions))]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetCompetitionsForRegistration_test(string? competitionName, string? sportType, string? location, int expectedCount)
        {
            // Arrange
            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            ILogger<GetInscribirCompeticion> logger = mock.Object;
            var controller = new GetInscribirCompeticion(_context, logger);

            // Act
            var result = await controller.GetCompetitionsForRegistration(competitionName, sportType, location);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var competitionsActual = Assert.IsType<List<CompeticionParaInscribirseDTO>>(okResult.Value);
            Assert.Equal(expectedCount, competitionsActual.Count);
        }

        public static IEnumerable<object[]> TestCasesFor_GetCompetitions()
        {
            return new List<object[]>
            {
                new object[] { null, null, null, 2 },         // Sin filtros: devuelve las 2 competiciones
                new object[] { "Albacete", null, null, 1 },   // Filtrando por nombre exacto
                new object[] { null, "Running", null, 1 }     // Filtrando por tipo de deporte exacto
            };
        }

// Caso de error: se pide una inscripción cuyo Id no existe
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetCompeticionInscripcion_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            var controller = new GetInscribirCompeticion(_context, mock.Object);

            // Act
// Id 999, que no existe en la BD.
            var result = await controller.GetCompeticionInscripcion(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

// Caso correcto: la inscripción existe
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetCompeticionInscripcion_Found_test()
        {
            // Arrange
            var tipoDeporte = new TipoDeporte("Running", "Carrera", "Desc");
            _context.Add(tipoDeporte);
            _context.SaveChanges();

            var competicion = new Competicion("Parque", DateTime.Today.AddDays(10), 10, 15.0m, tipoDeporte.Id, "Albacete");
            competicion.CompeticionesInscritas = new List<CompeticionInscrita>();
            _context.Add(competicion);
            _context.SaveChanges();

            var usuario = new ApplicationUser
            {
                Id = "user1",
                UserName = "usuario1",
                Name = "NombreUsuario",
                Surname = "Apellidos",
                Email = "usuario1@uclm.es",
                DNI = "12345678A",
                PhoneNumber = "600000000",
                Sex = "Hombre"
            };
            _context.Add(usuario);
            _context.SaveChanges();

            var inscripcion = new Inscripcion(
                "usuario1", 
                "Apellidos", 
                "12345678A", 
                "600000000", 
                DateTime.Now, 
                (MetodoPago)MetodoPagoTypes.TarjetaCreditoDebito, 
                15.0m, 
                "DatosPago"
            );
            
            inscripcion.Cliente = usuario; 
            inscripcion.CompeticionesInscritas = new List<CompeticionInscrita>
            {
                new CompeticionInscrita(competicion.Id, 1, "Ninguna")
            };

            _context.Add(inscripcion);
            _context.SaveChanges();

            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            var controller = new GetInscribirCompeticion(_context, mock.Object);

            // Act
            var result = await controller.GetCompeticionInscripcion(inscripcion.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var detalleActual = Assert.IsType<CompeticionInscripcionDetailDTO>(okResult.Value);
            Assert.Equal(inscripcion.Id, detalleActual.Id);
        }

//caso error: el DTO llega sin ninguna competición
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateCompeticionInscripcion_EmptyList_Error_test()
        {
            // Arrange
            var dtoParaCrear = new CompeticionInscripcionForCreateDTO
            {
                NombreUsuario = "usuario1",
                ApellidosUsuario = "Apellidos",
                Dni = "12345678A",
                Telefono = "600000000",
                DatosPago = "DatosPago",
                MetodoPago = MetodoPagoTypes.TarjetaCreditoDebito,
                CompeticionesInscritas = new List<CompeticionInscritaCreateItemDTO>()
            };
            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            var controller = new GetInscribirCompeticion(_context, mock.Object);

            // Act
            var result = await controller.CreateCompeticionInscripcion(dtoParaCrear);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.True(problemDetails.Errors.ContainsKey("CompeticionesInscritas"));
        }

//caso de error: se pide una inscripción a una competición que no existe
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateCompeticionInscripcion_CompeticionNotFound_Error_test()
        {
            // Arrange
            var itemDto = new CompeticionInscritaCreateItemDTO
            {
                CompeticionId = 9999,
                ProblemasFisicos = "Ninguna"
            };

            var dtoParaCrear = new CompeticionInscripcionForCreateDTO
            {
                NombreUsuario = "usuario1",
                ApellidosUsuario = "Apellidos",
                Dni = "12345678A",
                Telefono = "600000000",
                DatosPago = "DatosPago",
                MetodoPago = MetodoPagoTypes.TarjetaCreditoDebito,
                CompeticionesInscritas = new List<CompeticionInscritaCreateItemDTO> { itemDto }
            };

            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            var controller = new GetInscribirCompeticion(_context, mock.Object);

            // Act
            var result = await controller.CreateCompeticionInscripcion(dtoParaCrear);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.True(problemDetails.Errors.ContainsKey("CompeticionesInscritas"));
        }

//caso de error: no hay plazas disponibles
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateCompeticionInscripcion_NoPlazas_Error_test()
        {
            // Arrange
            var tipoDeporte = new TipoDeporte("Running", "Carrera", "Desc");
            _context.Add(tipoDeporte);
            _context.SaveChanges();

            var competicion = new Competicion("Parque", DateTime.Today.AddDays(10), 0, 20.0m, tipoDeporte.Id, "Albacete");
            competicion.CompeticionesInscritas = new List<CompeticionInscrita>();
            _context.Add(competicion);
            _context.SaveChanges();

            var itemDto = new CompeticionInscritaCreateItemDTO
            {
                CompeticionId = competicion.Id,
                ProblemasFisicos = "Ninguna"
            };
            
            var dtoParaCrear = new CompeticionInscripcionForCreateDTO
            {
                NombreUsuario = "usuario1",
                ApellidosUsuario = "Apellidos",
                Dni = "12345678A",
                Telefono = "600000000",
                DatosPago = "DatosPago",
                MetodoPago = MetodoPagoTypes.TarjetaCreditoDebito,
                CompeticionesInscritas = new List<CompeticionInscritaCreateItemDTO> { itemDto }
            };

            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            var controller = new GetInscribirCompeticion(_context, mock.Object);

            // Act
            var result = await controller.CreateCompeticionInscripcion(dtoParaCrear);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.True(problemDetails.Errors.ContainsKey("CompeticionesInscritas"));
        }

//caso correcto: la inscripción se crea correctamente
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateCompeticionInscripcion_Success_test()
        {
            // Arrange
            var tipoDeporte = new TipoDeporte("Running", "Carrera", "Desc");
            _context.Add(tipoDeporte);
            _context.SaveChanges();

            var competicion = new Competicion("Parque", DateTime.Today.AddDays(10), 10, 20.0m, tipoDeporte.Id, "Albacete");
            competicion.CompeticionesInscritas = new List<CompeticionInscrita>();
            _context.Add(competicion);
            _context.SaveChanges();

            var usuario = _context.ApplicationUsers.FirstOrDefault(u => u.UserName == "usuario1");
            if (usuario == null)
            {
                usuario = new ApplicationUser
                {
                    Id = "user1",
                    UserName = "usuario1",
                    Name = "NombreUsuario",
                    Surname = "Apellidos",
                    Email = "usuario1@uclm.es",
                    DNI = "12345678A",
                    PhoneNumber = "600000000",
                    Sex = "Hombre"
                };
                _context.Add(usuario);
                _context.SaveChanges();
            }

            var itemDto = new CompeticionInscritaCreateItemDTO
            {
                CompeticionId = competicion.Id,
                ProblemasFisicos = "Ninguna"
            };
            
            var dtoParaCrear = new CompeticionInscripcionForCreateDTO
            {
                NombreUsuario = "usuario1",
                ApellidosUsuario = "Apellidos",
                Dni = "12345678A",
                Telefono = "600000000",
                DatosPago = "DatosPago",
                MetodoPago = MetodoPagoTypes.TarjetaCreditoDebito,
                CompeticionesInscritas = new List<CompeticionInscritaCreateItemDTO> { itemDto }
            };

            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            var controller = new GetInscribirCompeticion(_context, mock.Object);

            // Act
            var result = await controller.CreateCompeticionInscripcion(dtoParaCrear);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var detalleActual = Assert.IsType<CompeticionInscripcionDetailDTO>(createdResult.Value);
            Assert.Equal("usuario1", detalleActual.NombreUsuario);
            Assert.Single(detalleActual.CompeticionesInscritas);
        }

// Caso de error: se intenta crear una inscripción sin competiciones
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateCompeticionInscripcion_Conflict_test()
        {
            // Arrange
            var tipoDeporte = new TipoDeporte("Running", "Carrera", "Desc");
            _context.Add(tipoDeporte);
            _context.SaveChanges();

            var competicion = new Competicion("Parque", DateTime.Today.AddDays(10), 10, 20.0m, tipoDeporte.Id, "Albacete");
            competicion.CompeticionesInscritas = new List<CompeticionInscrita>();
            _context.Add(competicion);
            _context.SaveChanges();

            var usuario = new ApplicationUser
            {
                Id = "user1",
                UserName = "usuario1",
                Name = "NombreUsuario",
                Surname = "Apellidos",
                Email = "usuario1@uclm.es",
                DNI = "12345678A",
                PhoneNumber = "600000000",
                Sex = "Hombre"
            };
            _context.Add(usuario);
            _context.SaveChanges();

            // Añadimos una entidad huérfana al contexto para forzar una excepción de clave ajena en SaveChangesAsync
            var orphanInscrita = new CompeticionInscrita(competicion.Id, 9999, "Error Forzado");
            _context.Add(orphanInscrita);

            var itemDto = new CompeticionInscritaCreateItemDTO
            {
                CompeticionId = competicion.Id,
                ProblemasFisicos = "Ninguna"
            };
            
            var dtoParaCrear = new CompeticionInscripcionForCreateDTO
            {
                NombreUsuario = "usuario1",
                ApellidosUsuario = "Apellidos",
                Dni = "12345678A",
                Telefono = "600000000",
                DatosPago = "DatosPago",
                MetodoPago = MetodoPagoTypes.TarjetaCreditoDebito,
                CompeticionesInscritas = new List<CompeticionInscritaCreateItemDTO> { itemDto }
            };

            var mock = new Mock<ILogger<GetInscribirCompeticion>>();
            var controller = new GetInscribirCompeticion(_context, mock.Object);

            // Act
            var result = await controller.CreateCompeticionInscripcion(dtoParaCrear);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            string mensajeError = Assert.IsType<string>(conflictResult.Value);
            Assert.NotNull(mensajeError);
        }
    }
}