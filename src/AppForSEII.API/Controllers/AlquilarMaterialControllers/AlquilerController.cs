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
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            .Select(a => new AlquilerMaterialDETAILSDTO(a.IdAlquiler, a.Usuario.DNI, a.Usuario.Name, a.Usuario.Surname, a.Usuario.PhoneNumber, a.MetodoPago, a.FechaAlquiler,
            a.MaterialesAlquilados.Select(m => new ItemsMaterialDTO(m.Material.IdMaterial, m.Material.Nombre, m.Precio, m.Cantidad, m.Material.TipoMaterial.NombreTipoMaterial, m.Material.TipoDeporte.NombreTipoDeporte)).ToList()))
            .FirstOrDefaultAsync();
            
            if(alquiler == null)
            {
                _logger.LogError($"Error: alquiler con id: {id} no existe");
                return NotFound();
            }

            return Ok(alquiler);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(AlquilerMaterialDETAILSDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> PostAlquilerMaterial(AlquilerMaterialPOSTDTO alquilerCr)
        {
            
            if(alquilerCr.FechaAlquiler < DateTime.Today)
            {
                ModelState.AddModelError("FechaRenta", "Fecha no valida");
            }

            if(alquilerCr.MaterialesAlquilados.Count() < 1)
            {
                ModelState.AddModelError("FechaRenta", "Minimo un objeto a alquilar");
            }

            var user = _context.ApplicationUsers.FirstOrDefault(au => au.Name == alquilerCr.NombreUsuario);
            if (user == null)
                ModelState.AddModelError("RentalApplicationUser", "Error! UserName is not registered");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            var nombreMateriales = alquilerCr.MaterialesAlquilados.Select(m => m.Nombre).ToList<string>();

            var materiales = _context.Materiales.Include(m => m.MaterialAlquilados).ThenInclude(a => a.Alquiler)
            .Where(m => nombreMateriales.Contains(m.Nombre)).ToList();

            Alquiler alquiler = new Alquiler(user, alquilerCr.FechaAlquiler, new List<MaterialAlquilado>(), alquilerCr.MetodoPago);
            
            

            foreach (var item in alquilerCr.MaterialesAlquilados)
            {
                Material? material = materiales.FirstOrDefault(m => m.Nombre == item.Nombre);

                if(material == null)
                {

                    ModelState.AddModelError("AlquilerItem", $"Material '{item.Nombre}' no esta disponible");

                }

                else if(material.Cantidad < item.Cantidad)
                {

                    ModelState.AddModelError("AlquilerItemCantidad", $"No hay stock suficiente de material '{item.Nombre}'");

                }

                else{
                    
                    MaterialAlquilado materialAq = new MaterialAlquilado(item.Cantidad, alquiler, material);
                    material.Cantidad -= item.Cantidad;
                    materialAq.Precio =  item.Precio;
                    alquiler.MaterialesAlquilados.Add(materialAq);
                    
                }

            }

            alquiler.PrecioTotal = (decimal) alquiler.MaterialesAlquilados.Sum(m => m.Precio * m.Cantidad);

            if (ModelState.ErrorCount > 0) {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            _context.Add(alquiler);

            try {
        
                await _context.SaveChangesAsync();

            }
            catch (Exception ex) {

                _logger.LogError(ex.Message);
                ModelState.AddModelError("Rental", $"Error! There was an error while saving your rental, plese, try again later");
                return Conflict("Error" + ex.Message);

            }

            var alquilerDetail = new AlquilerMaterialDETAILSDTO(alquiler.IdAlquiler, alquiler.Usuario.DNI, alquiler.Usuario.Name, alquiler.Usuario.Surname, alquiler.Usuario.PhoneNumber, 
            alquiler.MetodoPago, alquiler.FechaAlquiler, alquilerCr.MaterialesAlquilados);

            return CreatedAtAction("DetailsAlquilerMaterial", new { id = alquiler.IdAlquiler}, alquilerDetail);

        }
    }
}
