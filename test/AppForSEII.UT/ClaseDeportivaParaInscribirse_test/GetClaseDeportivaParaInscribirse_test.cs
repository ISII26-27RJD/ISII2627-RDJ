using AppForSEII.API.Controllers;
using AppForSEII.API.Models;
using AppForSEII.Shared.ClassDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AppForSEII.UT.Controllers_test
{
    // Clase de tests unitarios para el controlador GETClaseDeportivaParaInscribirse.
    // Hereda de AppForSEII4SqliteUT que configura una BD SQLite en memoria, 
    // aislando los tests de la BD real de SQL Server.
    public class GETClaseDeportivaParaInscribirse_test : AppForSEII4SqliteUT
    {
        // Constantes de datos de prueba para evitar repetir strings literales en cada test.
        private const string _userName = "elena.navarro@uclm.es";
        private const string _customerNameSurname = "Elena Navarro";
        private const string _deporte1Nombre = "Tenis";
        private const string _deporte2Nombre = "Natacion";
        private const string _clase1Nombre = "Tenis Iniciacion";
        private const string _clase2Nombre = "Tenis Avanzado";
        private const string _clase3Nombre = "Natacion Infantil";
        private const string _clase4Nombre = "Natacion Sin Plazas";

        // Constructor: se ejecuta antes de cada test para preparar el estado inicial de la BD en memoria.
        // Actua como el "Arrange" compartido por todos los tests de esta clase.
        public GETClaseDeportivaParaInscribirse_test()
        {
            // Crea los tipos de deporte necesarios para las clases.
            var tiposDeporte = new List<TipoDeporte>()
            {
                new TipoDeporte(_deporte1Nombre, "Deporte de " + _deporte1Nombre),
                new TipoDeporte(_deporte2Nombre, "Deporte de " + _deporte2Nombre)
            };
            _context.AddRange(tiposDeporte);
            _context.SaveChanges();

            // Crea 4 clases deportivas de prueba:
            // - Clase 1: Tenis Iniciacion (1 plaza) -> quedara sin plazas tras la inscripcion del setUp
            // - Clase 2: Tenis Avanzado (10 plazas) -> disponible
            // - Clase 3: Natacion Infantil (15 plazas) -> disponible
            // - Clase 4: Natacion Sin Plazas (0 plazas) -> no debe aparecer en el catalogo
            var clases = new List<ClaseDeportiva>()
            {
                //New ClaseDeportiva(descripcion, fechaHora, monitor, nivel, plazasDisponibles, precioUnitario, tipoDeporteId, lugar)
                new ClaseDeportiva(_clase1Nombre, DateTime.Today.AddDays(2), "Carlos Garcia", "Iniciacion", 1, 15.0m, tiposDeporte[0].Id, "Pista 1"),
                new ClaseDeportiva(_clase2Nombre, DateTime.Today.AddDays(3), "Carlos Garcia", "Avanzado", 10, 20.0m, tiposDeporte[0].Id, "Pista 1"),
                new ClaseDeportiva(_clase3Nombre, DateTime.Today.AddDays(4), "Ana Lopez", "Infantil", 15, 12.0m, tiposDeporte[1].Id, "Piscina"),
                new ClaseDeportiva(_clase4Nombre, DateTime.Today.AddDays(3), "Ana Lopez", "General", 0, 10.0m, tiposDeporte[1].Id, "Piscina")
            };
            _context.AddRange(clases);
            _context.SaveChanges();

            // Crea el usuario de prueba que se usara en los tests de inscripcion.

            //AplicacionUser(string id, string nombre, string apellidos, string userName, string dni, int edad, string genero)
            ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martinez", _userName, "12345678A", 30, "Femenino");
            _context.Add(user);
            _context.SaveChanges();

            // Crea una inscripcion preexistente en la BD para poder testear el GET detail.
    
            //new Inscripcion(string nombreUsuario, string apellidosUsuario, string dni, string telefono, DateTime fechaInscripcion, MetodoPagoTypes metodoPago, decimal precioTotal, string datosPago)
            
            var inscripcion = new Inscripcion(_userName, _customerNameSurname, "12345678A", "600123456", DateTime.Today, MetodoPago.Tarjeta, 15.0m, "**** **** **** 1234");
            inscripcion.Cliente = user;
            inscripcion.ClasesInscritas = new List<ClaseInscrita>
            {
                new ClaseInscrita(clases[0].Id, 0, 1, 15.0m, "Mi clase favorita")
            };

            // Se descuenta la plaza reservada de las disponibles en la clase (simula la logica del POST).
            clases[0].PlazasDisponibles -= 1;

            _context.Add(inscripcion);
            _context.SaveChanges();
        }

        // Metodo estatico que genera los casos de prueba para el test parametrizado del GET catalogo.
        // Cada caso define: filtros de entrada + lista de DTOs esperada como resultado.
        public static IEnumerable<object?[]> TestCasesFor_GetClasesDeportivasParaInscribirse_OK()
        {
            // DTOs de referencia que representan el estado esperado de las clases en la BD tras el setUp.
            // Clase 1 tiene 0 plazas (restadas en el constructor) -> NO aparecera en el catalogo.
            var claseDTOs = new List<ClaseDeportivaParaInscribirseDTO>()
            {
                new ClaseDeportivaParaInscribirseDTO(1, _clase1Nombre, _deporte1Nombre, "Carlos Garcia", "Iniciacion", DateTime.Today.AddDays(2), "Pista 1", 0, 15.0m),
                new ClaseDeportivaParaInscribirseDTO(2, _clase2Nombre, _deporte1Nombre, "Carlos Garcia", "Avanzado", DateTime.Today.AddDays(3), "Pista 1", 10, 20.0m),
                new ClaseDeportivaParaInscribirseDTO(3, _clase3Nombre, _deporte2Nombre, "Ana Lopez", "Infantil", DateTime.Today.AddDays(4), "Piscina", 15, 12.0m)
            };

            // TC1: sin filtros -> clases 2 y 3 (la 1 tiene 0 plazas, la 4 tiene 0 plazas -> excluidas)
            var clasesTC1 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1], claseDTOs[2] }
                .OrderBy(c => c.Descripcion).ToList();

            // TC2: filtro por descripcion "Avanzado" -> solo clase 2
            var clasesTC2 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1] };

            // TC3: filtro por tipo de deporte "Natacion" -> solo clase 3
            var clasesTC3 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[2] };

            // TC4: filtro de fecha exacta (fromDate == toDate == +3 dias) -> solo clase 2 (el dia 4 queda fuera)
            var clasesTC4 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1] };

            // TC5: solo fromDate (+3 dias) -> clases 2 y 3 (la 3 esta en +4, dentro del rango)
            var clasesTC5 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1], claseDTOs[2] }
                .OrderBy(c => c.Descripcion).ToList();

            // TC6: solo toDate (+3 dias) -> solo clase 2 (la 3 esta en +4, fuera del rango)
            var clasesTC6 = new List<ClaseDeportivaParaInscribirseDTO>() { claseDTOs[1] };

            // Devuelve todos los casos como array de arrays para el [Theory] de xUnit.
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

        // Test parametrizado [Theory] que ejecuta GetClasesDeportivasParaInscribirse con distintos filtros.
        // Verifica que el resultado sea 200 OK y que los DTOs devueltos coincidan con los esperados.
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
            // Arrange: se crea un mock del logger (no queremos logs reales en los tests).
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            // Act: se llama al metodo del controlador con los parametros del caso de prueba.
            var result = await controller.GetClasesDeportivasParaInscribirse(filtroNombre, filtroTipoDeporte, fromDate, toDate);

            // Assert: comprueba que la respuesta es 200 OK y que la lista de DTOs coincide con la esperada.
            var okResult = Assert.IsType<OkObjectResult>(result);
            var claseDTOsActual = Assert.IsType<List<ClaseDeportivaParaInscribirseDTO>>(okResult.Value);
            Assert.Equal(expectedClases, claseDTOsActual);
        }

        // Test que verifica que GetClasesDeportivasParaInscribirse devuelve 400 BadRequest
        // cuando fromDate es posterior a toDate (rango de fechas invalido).
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetClasesDeportivasParaInscribirse_badrequest_test()
        {
            // Arrange
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            // Act: fromDate (+5 dias) > toDate (+1 dia) -> debe fallar la validacion.
            var result = await controller.GetClasesDeportivasParaInscribirse(
                null, null, DateTime.Today.AddDays(5), DateTime.Today.AddDays(1));

            // Assert: comprueba que el error de validacion tiene el mensaje esperado.
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var problem = problemDetails.Errors.First().Value[0];
            Assert.Equal("fromDate must be earlier than toDate", problem);
        }

        // Test que verifica que GetInscripcionClaseDeportiva devuelve 404 NotFound
        // cuando el Id proporcionado no existe en la BD.
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetInscripcionClaseDeportiva_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            // Act: se busca la inscripcion con Id=0, que no existe.
            var result = await controller.GetInscripcionClaseDeportiva(0);

            // Assert: debe devolver 404 NotFound.
            Assert.IsType<NotFoundResult>(result);
        }

        // Test que verifica que GetInscripcionClaseDeportiva devuelve 200 OK con el detalle correcto
        // cuando el Id existe en la BD (inscripcion creada en el constructor setUp).
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetInscripcionClaseDeportiva_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            // DTO esperado: refleja exactamente la inscripcion creada en el constructor setUp.
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

            // Act: se busca la inscripcion con Id=1 (la creada en el setUp).
            var result = await controller.GetInscripcionClaseDeportiva(1);

            // Assert: comprueba 200 OK y que el DTO devuelto es igual al esperado (comparacion por valor via Equals).
            var okResult = Assert.IsType<OkObjectResult>(result);
            var inscripcionDTOActual = Assert.IsType<ClaseDeportivaInscripcionDetailDTO>(okResult.Value);
            Assert.Equal(expectedInscripcion, inscripcionDTOActual);
        }

        // Metodo estatico que genera los casos de prueba de error para el POST CreateInscripcionClaseDeportiva.
        // Cada caso define: DTO de entrada con datos erroneos + mensaje de error esperado.
        public static IEnumerable<object[]> TestCasesFor_CreateInscripcion()
        {

        
        //ClaseDeportivaInscripcionForCreateDTO(string nombreUsuario, string apellidosUsuario, string dni, string telefono, string datosPago, MetodoPagoTypes metodoPago, IList<ClassInscriptionCreateItemDTO> clasesInscritas)
        //new ClassInscriptionCreateItemDTO(int claseDeportivaId, int plazasReservadas, string? observaciones)


            // Caso 1: lista de clases vacia -> debe rechazarse.
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

            // Caso 2: ClasesInscritas = null -> debe rechazarse igual que lista vacia.
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

            // Caso 3: usuario no registrado en la BD -> debe indicar que el usuario no existe.
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

            // Caso 4: ID de clase deportiva que no existe en la BD (999) -> clase no disponible.
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

            // Caso 5: clase sin plazas disponibles (Id=4, creada con 0 plazas en el setUp) -> no disponible.
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

            // Devuelve la coleccion de casos: [DTO de entrada, mensaje de error esperado].
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

        // Test parametrizado [Theory] que verifica que el POST devuelve 400 BadRequest
        // para cada uno de los 5 casos de error definidos en TestCasesFor_CreateInscripcion.
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateInscripcion))]
        public async Task CreateInscripcionClaseDeportiva_Error_test(
            ClaseDeportivaInscripcionForCreateDTO inscripcionDTO,
            string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            // Act: llama al POST con el DTO erroneo.
            var result = await controller.CreateInscripcionClaseDeportiva(inscripcionDTO);

            // Assert: verifica 400 BadRequest y que el mensaje de error empieza con el texto esperado.
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var errorActual = problemDetails.Errors.First().Value[0];
            Assert.StartsWith(errorExpected, errorActual);
        }

        // Test [Fact] que verifica el happy path del POST: inscripcion creada correctamente -> 201 Created.
        // Elena reserva 2 plazas en "Tenis Avanzado" (Id=2) pagando 40€ (20€/plaza x 2).
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateInscripcionClaseDeportiva_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<GETClaseDeportivaParaInscribirse>>();
            ILogger<GETClaseDeportivaParaInscribirse> logger = mock.Object;
            var controller = new GETClaseDeportivaParaInscribirse(_context, logger);

            // Items que se van a inscribir: 2 plazas en Tenis Avanzado.
            var itemsParaInscribir = new List<ClassInscriptionCreateItemDTO>()
            {
                new ClassInscriptionCreateItemDTO { ClaseDeportivaId = 2, PlazasReservadas = 2, Observaciones = "Con ganas de empezar" }
            };

            // DTO de entrada con todos los datos del usuario y las clases.
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

            // DTO de respuesta esperado: Id=2 (es la segunda inscripcion en la BD), precio=40€ (20€ x 2 plazas).
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

            // Act: llama al POST con el DTO correcto.
            var result = await controller.CreateInscripcionClaseDeportiva(inscripcionDTO);

            // Assert: comprueba 201 Created y que el detalle devuelto coincide con el esperado.
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualDetailDTO = Assert.IsType<ClaseDeportivaInscripcionDetailDTO>(createdResult.Value);
            Assert.Equal(expectedDetailDTO, actualDetailDTO);
        }
    }
}
