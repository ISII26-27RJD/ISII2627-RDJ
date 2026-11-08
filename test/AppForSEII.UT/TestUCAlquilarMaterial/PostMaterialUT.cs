using AppForSEII.API.Controllers.AlquilarMaterialControllers;
using AppForSEII.API.DTOs.AlquilarMaterialDTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppForSEII.UT.TestUCAlquilarMaterial
{

    public class PostMaterialUT : AppForSEII4SqliteUT
    {

        private const string nombre = "Elena";
        private const string apellidos = "Navarro Martínez";
        private const string telefono = "123445566";
        private const string DNI = "11111111A";
        private const Alquiler.MetodoPagoTipos metodoPago = Alquiler.MetodoPagoTipos.Tarjeta;

        

        public PostMaterialUT()
        {
            
            var tipoMaterial = new List<TipoMaterial>()
            {

                new TipoMaterial("Seguridad"),
                new TipoMaterial("Entrenamiento")

            };

            var tipoDeporte = new List<TipoDeporte>()
            {

                new TipoDeporte("Futbol", "Futbol"),
                new TipoDeporte("Tenis", "Tenis", null)

            };

            var material = new List<Material>()
            {

                new Material(10,"Rodilleras", 1,tipoMaterial[0], tipoDeporte[0]),
                new Material(10,"Raqueta", 2, tipoMaterial[1], tipoDeporte[1])

            };

            ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
            user.PhoneNumber = "123445566";
            user.DNI = "11111111A";

            var alquiler = new Alquiler(user, DateTime.Now, new List<MaterialAlquilado>(), Alquiler.MetodoPagoTipos.Tarjeta);

            alquiler.MaterialesAlquilados.Add(new MaterialAlquilado(1, alquiler, material[0]));

            _context.Add(user);
            _context.AddRange(tipoMaterial);
            _context.AddRange(tipoDeporte);
            _context.AddRange(material);
            _context.SaveChanges();

        }

        public static IEnumerable<object[]> postAlquilerTests()
        {
            
            var itemsBien = new List<ItemsMaterialDTO>(){new ItemsMaterialDTO(1, "Rodilleras", (decimal)1.0, 1, "Seguridad", "Futbol")};

            var itemsMal1 = new List<ItemsMaterialDTO>(){new ItemsMaterialDTO(1, "Barra de pan", (decimal)1.0, 1, "Seguridad", "Futbol")};

            var itemsMal2 = new List<ItemsMaterialDTO>(){new ItemsMaterialDTO(1, "Rodilleras", (decimal)1.0, 9999, "Seguridad", "Futbol")};

            var fechaErronea = new AlquilerMaterialPOSTDTO(DNI, nombre, apellidos, telefono, metodoPago, DateTime.Now.AddDays(-1) , new List<ItemsMaterialDTO>());

            var numeroMaterialesErroneo = new AlquilerMaterialPOSTDTO(DNI, nombre, apellidos, telefono, metodoPago, DateTime.Now , new List<ItemsMaterialDTO>());

            var usuarioErroneo = new AlquilerMaterialPOSTDTO(DNI, "JoseJuan", apellidos, telefono, metodoPago, DateTime.Now , itemsBien);

            var materialErroneo = new AlquilerMaterialPOSTDTO(DNI, nombre, apellidos, telefono, metodoPago, DateTime.Now , itemsMal1);

            var cantidadErroneo = new AlquilerMaterialPOSTDTO(DNI, nombre, apellidos, telefono, metodoPago, DateTime.Now , itemsMal2);

             var allTests = new List<object[]>
            {             
                new object[] { fechaErronea, "Fecha no valida",  },
                new object[] { numeroMaterialesErroneo, "Minimo un objeto a alquilar", },
                new object[] { usuarioErroneo, "Error! UserName is not registered", },
                new object[] { materialErroneo, "Material 'Barra de pan' no esta disponible", },
                new object[] { cantidadErroneo, "No hay stock suficiente de material 'Rodilleras'", },
            };

            return allTests;

        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(postAlquilerTests))]
        public async Task CreateRental_Error_test(AlquilerMaterialPOSTDTO alquilerDTO, string errorExpected)
        {
            
            //Arrange
            var mock = new Mock<ILogger<AlquilerController>>();
            ILogger<AlquilerController> logger = mock.Object;

            var controller = new AlquilerController(_context, logger);

            // Act
            var result = await controller.PostAlquilerMaterial(alquilerDTO);

            //Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            Assert.StartsWith(errorExpected, errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateRental_Success_test()
        {
            //Arrange
            var mock = new Mock<ILogger<AlquilerController>>();
            ILogger<AlquilerController> logger = mock.Object;

            var controller = new AlquilerController(_context, logger);

            var alquilerDTO = new AlquilerMaterialPOSTDTO(DNI, nombre, apellidos, telefono, metodoPago, DateTime.Now, 
            new List<ItemsMaterialDTO>(){ new ItemsMaterialDTO(1, "Rodilleras", (decimal)1.0, 1, "Seguridad", "Futbol") });

            var expected = new AlquilerMaterialDETAILSDTO(1,DNI, nombre, apellidos, telefono, metodoPago, DateTime.Now, 
            new List<ItemsMaterialDTO>(){new ItemsMaterialDTO(1, "Rodilleras", (decimal)1.0, 1, "Seguridad", "Futbol")});

            //Act
            var result = await controller.PostAlquilerMaterial(alquilerDTO);

            //Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actual = Assert.IsType<AlquilerMaterialDETAILSDTO>(createdResult.Value);

            Assert.Equal(expected, actual);

        }  
    }
}