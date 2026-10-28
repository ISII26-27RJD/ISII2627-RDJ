using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.DTOs.ApplicationUserDTO;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.AlquilarMaterialDTOs;
using NuGet.ProjectModel;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(ApplicationDbContext context, ILogger<MaterialController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<MaterialAlquilarDTO>),(int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails),(int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetMaterialAlquilar(decimal ?precio, string ?material)
        {
            
            IList<MaterialAlquilarDTO> selectMateriales = await _context.Materiales.Include(m => m.TipoMaterial).Include(m => m.TipoDeporte).Where(m =>(precio == null || m.Precio <= precio) && 
            (material== null || m.TipoMaterial.NombreTipoMaterial.Equals(material))).OrderBy(m => m.Nombre)
            .Select(m => new MaterialAlquilarDTO(m.IdMaterial, m.Nombre, m.Precio, m.TipoMaterial.NombreTipoMaterial, m.TipoDeporte.Nombre)).ToListAsync();

            return Ok(selectMateriales);
        }
    }
}
