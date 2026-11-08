using AppForSEII.API.Controllers;
using AppForSEII.API.Models;
using AppForSEII.Shared.ClassDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AppForSEII.UT.Controllers_test
{
    public class GETClaseDeportivaParaInscribirse_test : AppForSEII4SqliteUT
    {
        private const string _userName = "elena.navarro@uclm.es";
        private const string _customerNameSurname = "Elena Navarro";
        private const string _deporte1Nombre = "Tenis";
        private const string _deporte2Nombre = "Natacion";
        private const string _clase1Nombre = "Tenis Iniciacion";
        private const string _clase2Nombre = "Tenis Avanzado";
        private const string _clase3Nombre = "Natacion Infantil";
        private const string _clase4Nombre = "Natacion Sin Plazas";

        public GETClaseDeportivaParaInscribirse_test()
        {
            var tiposDeporte = new List<TipoDeporte>()
            {
                new TipoDeporte(_deporte1Nombre, "Deporte de " + _deporte1Nombre),
                new TipoDeporte(_deporte2Nombre, "Deporte de " + _deporte2Nombre)
            };
            _context.AddRange(tiposDeporte);
            _context.SaveChanges();

            var clases = new List<ClaseDeportiva>()
            {
                new ClaseDeportiva(_clase1Nombre, DateTime.Today.AddDays(2), "Carlos Garcia", "Iniciacion", 1, 15.0m, tiposDeporte[0].Id, "Pista 1"),
                new ClaseDeportiva(_clase2Nombre, DateTime.Today.AddDays(3), "Carlos Garcia", "Avanzado", 10, 20.0m, tiposDeporte[0].Id, "Pista 1"),
                new ClaseDeportiva(_clase3Nombre, DateTime.Today.AddDays(4), "Ana Lopez", "Infantil", 15, 12.0m, tiposDeporte[1].Id, "Piscina"),
                new ClaseDeportiva(_clase4Nombre, DateTime.Today.AddDays(3), "Ana Lopez", "General", 0, 10.0m, tiposDeporte[1].Id, "Piscina")
            };
            _context.AddRange(clases);
            _context.SaveChanges();

            ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martinez", _userName, "12345678A", 30, "Femenino");
            _context.Add(user);
            _context.SaveChanges();

            var inscripcion = new Inscripcion(
                _userName,
                _customerNameSurname,
                "12345678A",
                "600123456",
                DateTime.Today,
                MetodoPago.Tarjeta,
                15.0m,
                "**** **** **** 1234");
            inscripcion.Cliente = user;
            inscripcion.ClasesInscritas = new List<ClaseInscrita>
            {
                new ClaseInscrita(clases[0].Id, 0, 1, 15.0m, "Mi clase favorita")
            };

            clases[0].PlazasDisponibles -= 1;

            _context.Add(inscripcion);
            _context.SaveChanges();
        }

        public static IEnumerable<object?[]> TestCasesFor_GetClasesDeportivasParaInscribirse_OK()
        {
            var claseDTOs = new List<ClaseDeportivaParaInscribirseDTO>()
            {
                new ClaseDeportivaParaInscribirseDTO(1, _clase1Nombre, _deporte1Nombre, "Carlos Garcia", "Iniciacion", DateTime.Today.AddDays(2), "Pista 1", 0, 15.0m),
                new ClaseDeportivaParaInscribirseDTO(2, _clase2Nombre, _deporte1Nombre, "Carlos Garcia", "Avanzado", DateTime.Today.AddDays(3), "Pista 1", 10, 20.0m),
                new ClaseDeportivaParaInscribirseDTO(3, _clase3Nombre, _deporte2Nombre, "Ana Lopez", "Infantil", DateTime.Today.AddDays(4), "Piscina", 15, 12.0m)
            };

            var clasesTC1 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1], claseDTOs[2] }
                .OrderBy(c => c.Descripcion).ToList();

            var clasesTC2 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1] };

            var clasesTC3 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[2] };

            var clasesTC4 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1] };

            var clasesTC5 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1], claseDTOs[2] }
                .OrderBy(c => c.Descripcion).ToList();

            var clasesTC6 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1] };

            var allTests = new List<object?[]>
            {
                new object?[] { null, null, null, null, clasesTC1 },
                new object?[] { "Avanzado", null, null, null, clasesTC2 },
                new object?[] { null, _deporte2Nombre, null, null, clasesTC3 },
                new object?[] { null, null, DateTime.Today.AddDays(3), DateTime.Today.AddDays(3), clasesTC4 },
                new object?[] { null, null, DateTime.Today.AddDays(3), null, clasesTC5 },
                new object?[] { null, null, null, DateTime.Today.AddDays(3), clasesTC6 }
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetClasesDeportivasParaInscribirse_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetClasesDeportivasParaInscribirse_OK_test(
            string? filtroNombre,
            string? filtroTipoDeporte,
            DateTime? fromDate,
            DateTime? toDate,
            IList<ClaseDeportivaParaInscribirseDTO> expectedClases)
        {
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            var result = await controller.GetClasesDeportivasParaInscribirse(filtroNombre, filtroTipoDeporte, fromDate, toDate);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var claseDTOsActual = Assert.IsType<List<ClaseDeportivaParaInscribirseDTO>>(okResult.Value);
            Assert.Equal(expectedClases, claseDTOsActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetClasesDeportivasParaInscribirse_badrequest_test()
        {
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            var result = await controller.GetClasesDeportivasParaInscribirse(
                null, null, DateTime.Today.AddDays(5), DateTime.Today.AddDays(1));

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var problem = problemDetails.Errors.First().Value[0];
            Assert.Equal("fromDate must be earlier than toDate", problem);
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetInscripcionClaseDeportiva_NotFound_test()
        {
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            var result = await controller.GetInscripcionClaseDeportiva(0);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetInscripcionClaseDeportiva_Found_test()
        {
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            var expectedInscripcion = new ClaseDeportivaInscripcionDetailDTO(
                1,
                DateTime.Today,
                _userName,
                _customerNameSurname,
                "12345678A",
                "600123456",
                "**** **** **** 1234",
                MetodoPagoTypes.Tarjeta,
                15.0m,
                new List<ClassInscriptionItemDTO>()
                {
                    new ClassInscriptionItemDTO(1, _clase1Nombre, _deporte1Nombre, DateTime.Today.AddDays(2), 1, 15.0m, "Mi clase favorita")
                });

            var result = await controller.GetInscripcionClaseDeportiva(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var inscripcionDTOActual = Assert.IsType<ClaseDeportivaInscripcionDetailDTO>(okResult.Value);
            Assert.Equal(expectedInscripcion, inscripcionDTOActual);
        }

        public static IEnumerable<object[]> TestCasesFor_CreateInscripcion()
        {
            var inscripcionSinItems = new ClaseDeportivaInscripcionForCreateDTO
            {
                NombreUsuario = _userName,
                ApellidosUsuario = _customerNameSurname,
                Dni = "12345678A",
                Telefono = "600123456",
                DatosPago = "**** **** **** 1234",
                MetodoPago = MetodoPagoTypes.Tarjeta,
                ClasesInscritas = new List<ClassInscriptionCreateItemDTO>()
            };

            var inscripcionClasesNull = new ClaseDeportivaInscripcionForCreateDTO
            {
                NombreUsuario = _userName,
                ApellidosUsuario = _customerNameSurname,
                Dni = "12345678A",
                Telefono = "600123456",
                DatosPago = "**** **** **** 1234",
                MetodoPago = MetodoPagoTypes.Tarjeta,
                ClasesInscritas = null!
            };

            var inscripcionUsuarioNoRegistrado = new ClaseDeportivaInscripcionForCreateDTO
            {
                NombreUsuario = "usuario_inexistente",
                ApellidosUsuario = "Desconocido",
                Dni = "99999999Z",
                Telefono = "600000000",
                DatosPago = "**** **** **** 1234",
                MetodoPago = MetodoPagoTypes.Tarjeta,
                ClasesInscritas = new List<ClassInscriptionCreateItemDTO>
                {
                    new ClassInscriptionCreateItemDTO { ClaseDeportivaId = 2, PlazasReservadas = 1, Observaciones = "Obs" }
                }
            };

            var inscripcionClaseNoExiste = new ClaseDeportivaInscripcionForCreateDTO
            {
                NombreUsuario = _userName,
                ApellidosUsuario = _customerNameSurname,
                Dni = "12345678A",
                Telefono = "600123456",
                DatosPago = "**** **** **** 1234",
                MetodoPago = MetodoPagoTypes.Tarjeta,
                ClasesInscritas = new List<ClassInscriptionCreateItemDTO>
                {
                    new ClassInscriptionCreateItemDTO { ClaseDeportivaId = 999, PlazasReservadas = 1, Observaciones = "Observacion" }
                }
            };

            var inscripcionClaseSinPlazas = new ClaseDeportivaInscripcionForCreateDTO
            {
                NombreUsuario = _userName,
                ApellidosUsuario = _customerNameSurname,
                Dni = "12345678A",
                Telefono = "600123456",
                DatosPago = "**** **** **** 1234",
                MetodoPago = MetodoPagoTypes.Tarjeta,
                ClasesInscritas = new List<ClassInscriptionCreateItemDTO>
                {
                    new ClassInscriptionCreateItemDTO { ClaseDeportivaId = 4, PlazasReservadas = 1, Observaciones = "Observacion" }
                }
            };

            var allTests = new List<object[]>
            {
                new object[] { inscripcionSinItems, "Error! Debes incluir al menos una clase deportiva" },
                new object[] { inscripcionClasesNull, "Error! Debes incluir al menos una clase deportiva" },
                new object[] { inscripcionUsuarioNoRegistrado, "Error! El nombre de usuario no esta registrado" },
                new object[] { inscripcionClaseNoExiste, "Error! La clase deportiva '999' no esta disponible" },
                new object[] { inscripcionClaseSinPlazas, "Error! La clase deportiva '4' no esta disponible" }
            };

            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateInscripcion))]
        public async Task CreateInscripcionClaseDeportiva_Error_test(
            ClaseDeportivaInscripcionForCreateDTO inscripcionDTO,
            string errorExpected)
        {
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            var result = await controller.CreateInscripcionClaseDeportiva(inscripcionDTO);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var errorActual = problemDetails.Errors.First().Value[0];
            Assert.StartsWith(errorExpected, errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateInscripcionClaseDeportiva_Success_test()
        {
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            var itemsParaInscribir = new List<ClassInscriptionCreateItemDTO>()
            {
                new ClassInscriptionCreateItemDTO { ClaseDeportivaId = 2, PlazasReservadas = 2, Observaciones = "Con ganas de empezar" }
            };

            var inscripcionDTO = new ClaseDeportivaInscripcionForCreateDTO
            {
                NombreUsuario = _userName,
                ApellidosUsuario = _customerNameSurname,
                Dni = "12345678A",
                Telefono = "600123456",
                DatosPago = "**** **** **** 1234",
                MetodoPago = MetodoPagoTypes.Tarjeta,
                ClasesInscritas = itemsParaInscribir
            };

            var expectedDetailDTO = new ClaseDeportivaInscripcionDetailDTO(
                2,
                DateTime.Today,
                _userName,
                _customerNameSurname,
                "12345678A",
                "600123456",
                "**** **** **** 1234",
                MetodoPagoTypes.Tarjeta,
                40.0m,
                new List<ClassInscriptionItemDTO>()
                {
                    new ClassInscriptionItemDTO(2, _clase2Nombre, _deporte1Nombre, DateTime.Today.AddDays(3), 2, 40.0m, "Con ganas de empezar")
                });

            var result = await controller.CreateInscripcionClaseDeportiva(inscripcionDTO);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualDetailDTO = Assert.IsType<ClaseDeportivaInscripcionDetailDTO>(createdResult.Value);
            Assert.Equal(expectedDetailDTO, actualDetailDTO);
        }
    }
}
