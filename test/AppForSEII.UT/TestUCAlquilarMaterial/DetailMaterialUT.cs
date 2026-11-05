using AppForSEII.API.Controllers.AlquilarMaterialControllers;
using AppForSEII.API.DTOs.AlquilarMaterialDTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForSEII.UT.TestUCAlquilarMaterial
{
    
    public class DetailMaterialUT : AppForSEII4SqliteUT{
        
        public DetailMaterialUT()
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
            _context.Add(alquiler);
            _context.SaveChanges();

        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task DetailAlquiler_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<AlquilerController>>();
            ILogger<AlquilerController> logger = mock.Object;

            var controller = new AlquilerController(_context, logger);

            // Act
            var result = await controller.DetailsAlquilerMaterial(0);

            //Assert
            //we check that the response type is OK and obtain the list of movies
            Assert.IsType<NotFoundResult>(result);

        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task DetailAlquilar_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<AlquilerController>>();
            ILogger<AlquilerController> logger = mock.Object;
            var controller = new AlquilerController(_context, logger);


            var expected = new AlquilerMaterialDETAILSDTO(1, "11111111A", "Elena", "Navarro Martínez", "123445566", 
            Alquiler.MetodoPagoTipos.Tarjeta, DateTime.Now, new List<ItemsMaterialDTO>());
            expected.MaterialesAlquilados.Add(new ItemsMaterialDTO(1, "Rodilleras", (decimal)1.0, 1, "Seguridad", "Futbol"));

            // Act 
            var result = await controller.DetailsAlquilerMaterial(1);

            //Assert
            //we check that the response type is OK and obtain the rental
            var okResult = Assert.IsType<OkObjectResult>(result);
            var Actual = Assert.IsType<AlquilerMaterialDETAILSDTO>(okResult.Value);
            var eq = expected.Equals(Actual);
            //we check that the expected and actual are the same
            Assert.Equal(expected, Actual);

        }

    }
}