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

namespace AppForSEII.API.Controllers.AlquilarMaterialControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlquilerController : ControllerBase
    {
        
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlquilerController> _logger;

        public AlquilerController(ApplicationDbContext context, ILogger<AlquilerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(AlquilerMaterialDETAILSDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DetailsAlquilerMaterial(int id)
        {
            var alquiler = await _context.Alquileres.Where(a => a.IdAlquiler == id).Include(m => m.MaterialesAlquilados).ThenInclude(m => m.Material)
            .Select(a => new AlquilerMaterialDETAILSDTO(a.IdAlquiler,a.DNI,a.NombreUsuario, a.ApellidosUsuario,a.NumeroTelefono,a.MetodoPago,a.FechaAlquiler,
            a.MaterialesAlquilados.Select(m => new ItemsMaterialDTO(m.Material.IdMaterial, m.Material.Nombre, m.Precio, m.Cantidad, m.Material.TipoMaterial.NombreTipoMaterial, m.Material.TipoDeporte.NombreTipoDeporte)).ToList()))
            .FirstOrDefaultAsync();
            
            if(alquiler == null)
            {
                _logger.LogError($"Error: alquiler con id: {id} no existe");
                return NotFound();
            }

            return Ok(alquiler);
        }

    }
}