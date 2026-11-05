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
    
    public class GetMaterialUT : AppForSEII4SqliteUT{
        
        public GetMaterialUT()
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

                new Material(10, "Rodilleras", 1, tipoMaterial[0], tipoDeporte[0]),
                new Material(10, "Raqueta", 2, tipoMaterial[1], tipoDeporte[1])

            };

            _context.AddRange(tipoMaterial);
            _context.AddRange(tipoDeporte);
            _context.AddRange(material);
            _context.SaveChanges();

        }

        public static IEnumerable<object?[]> getMaterialTests_Ok()
        {
            var materialAluilarDTO = new List<MaterialAlquilarDTO>()
            {
                
                new MaterialAlquilarDTO(1, "Rodilleras", 1,"Seguridad", "Futbol"),
                new MaterialAlquilarDTO(2, "Raqueta", 2, "Entrenamiento", "Tenis")

            };

            var materialDTOTC1 = new List<MaterialAlquilarDTO>(){ materialAluilarDTO[0], materialAluilarDTO[1] }.OrderBy(m => m.Nombre).ToList();
            
            var materialDTOTC2 = new List<MaterialAlquilarDTO>(){ materialAluilarDTO[0]};

            var materialDTOTC3 = new List<MaterialAlquilarDTO>(){ materialAluilarDTO[1]};

            var allTests = new List<object?[]>
            {
                
                new object?[] { null, null, materialDTOTC1 },
                new object?[] { (decimal)1, null, materialDTOTC2 },
                new object?[] { null, "Entrenamiento", materialDTOTC3 }

            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(getMaterialTests_Ok))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task Getmaterial_Ok_Test(decimal? filtroP, string? filtroN, IList<MaterialAlquilarDTO> expectedM)
        {
            
            //Arrange
            var mock = new Mock<ILogger<MaterialController>>();
            ILogger<MaterialController> logger = mock.Object;
            var controller = new MaterialController(_context, logger);

            //Act
            var result = await controller.GetMaterialAlquilar(filtroP, filtroN);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualDTO = Assert.IsType<List<MaterialAlquilarDTO>>(okResult.Value);
            Assert.Equal(expectedM, actualDTO);
        }

    }
}