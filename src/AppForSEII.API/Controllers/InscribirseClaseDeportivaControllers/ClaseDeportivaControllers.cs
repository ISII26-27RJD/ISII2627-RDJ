using AppForSEII.API.Data;
using AppForSEII.API.Models;
using AppForSEII.Shared.ClassDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GETClaseDeportivaParaInscribirse : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GETClaseDeportivaParaInscribirse> _logger;

        public GETClaseDeportivaParaInscribirse(ApplicationDbContext context, ILogger<GETClaseDeportivaParaInscribirse> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<ClaseDeportivaParaInscribirseDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetClasesDeportivasParaInscribirse(
            string? filtroNombre, 
            string? filtroTipoDeporte, 
            DateTime? fromDate, 
            DateTime? toDate)
        {
            if (fromDate != null && toDate != null && fromDate.Value > toDate.Value)
            {
                ModelState.AddModelError("fromDate", "fromDate must be earlier than toDate");
                _logger.LogError($"{DateTime.Now} Error: fromDate must be earlier than toDate");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var query = _context.ClasesDeportivas
                .Include(c => c.TipoDeporte)
                .Where(c => 
                    (filtroNombre == null || c.Descripcion.Contains(filtroNombre))
                    && (filtroTipoDeporte == null || c.TipoDeporte.Nombre.Equals(filtroTipoDeporte))
                    && (fromDate == null || c.FechaHora.Date >= fromDate.Value.Date)
                    && (toDate == null || c.FechaHora.Date <= toDate.Value.Date)
                    && (c.PlazasDisponibles > 0)
                )
                .OrderBy(c => c.Descripcion);

            var selectClasses = await query
                .Select(c => new ClaseDeportivaParaInscribirseDTO(
                    c.Id,
                    c.Descripcion,
                    c.TipoDeporte.Nombre,
                    c.Monitor,
                    c.Nivel,
                    c.FechaHora,
                    c.Lugar,
                    c.PlazasDisponibles,
                    c.PrecioUnitario
                ))
                .ToListAsync();

            return Ok(selectClasses);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ClaseDeportivaInscripcionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetInscripcionClaseDeportiva(int id)
        {
            var inscription = await _context.Inscripciones
                .Where(i => i.Id == id)
                .Include(i => i.ClasesInscritas)
                    .ThenInclude(ci => ci.ClaseDeportiva)
                        .ThenInclude(cd => cd.TipoDeporte)
                .Select(i => new ClaseDeportivaInscripcionDetailDTO(
                    i.Id,
                    i.FechaInscripcion,
                    i.NombreUsuario,
                    i.ApellidosUsuario,
                    i.DNI,
                    i.Telefono,
                    i.DatosPago,
                    (MetodoPagoTypes)i.MetodoPago,
                    i.PrecioTotal,
                    i.ClasesInscritas.Select(ci => new ClassInscriptionItemDTO(
                        ci.ClaseDeportiva.Id,
                        ci.ClaseDeportiva.Descripcion,
                        ci.ClaseDeportiva.TipoDeporte.Nombre,
                        ci.ClaseDeportiva.FechaHora,
                        ci.PlazasReservadas,
                        ci.Precio,
                        ci.Observaciones
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (inscription == null)
            {
                _logger.LogError($"Error: Inscripcion con id {id} no existe");
                return NotFound();
            }

            return Ok(inscription);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ClaseDeportivaInscripcionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CreateInscripcionClaseDeportiva(ClaseDeportivaInscripcionForCreateDTO inscripcionForCreate)
        {
            if (inscripcionForCreate.ClasesInscritas == null || inscripcionForCreate.ClasesInscritas.Count == 0)
            {
                ModelState.AddModelError("ClasesInscritas", "Error! Debes incluir al menos una clase deportiva para inscribirte");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var user = await _context.Users
                .OfType<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.UserName == inscripcionForCreate.NombreUsuario || u.DNI == inscripcionForCreate.Dni);

            if (user == null)
            {
                ModelState.AddModelError("NombreUsuario", "Error! El nombre de usuario no esta registrado");
            }

            var claseIds = inscripcionForCreate.ClasesInscritas.Select(ci => ci.ClaseDeportivaId).ToList();

            var classes = await _context.ClasesDeportivas
                .Include(c => c.TipoDeporte)
                .Where(c => claseIds.Contains(c.Id))
                .ToListAsync();

            var inscription = new Inscripcion(
                inscripcionForCreate.NombreUsuario,
                inscripcionForCreate.ApellidosUsuario,
                inscripcionForCreate.Dni,
                inscripcionForCreate.Telefono,
                DateTime.Now,
                (MetodoPago)(int)inscripcionForCreate.MetodoPago,
                0,
                inscripcionForCreate.DatosPago
            );

            if (user != null)
            {
                inscription.Cliente = user;
            }

            inscription.ClasesInscritas = new List<ClaseInscrita>();
            decimal totalPrice = 0;

            foreach (var item in inscripcionForCreate.ClasesInscritas)
            {
                var sportClass = classes.FirstOrDefault(c => c.Id == item.ClaseDeportivaId);

                if (sportClass == null || sportClass.PlazasDisponibles < item.PlazasReservadas)
                {
                    ModelState.AddModelError("ClasesInscritas", $"Error! La clase deportiva '{item.ClaseDeportivaId}' no esta disponible para inscribirse");
                }
                else
                {
                    decimal itemTotal = sportClass.PrecioUnitario * item.PlazasReservadas;
                    
                    inscription.ClasesInscritas.Add(new ClaseInscrita(
                        sportClass.Id, 
                        inscription.Id, 
                        item.PlazasReservadas, 
                        itemTotal, 
                        item.Observaciones
                    ));

                    sportClass.PlazasDisponibles -= item.PlazasReservadas;
                    totalPrice += itemTotal;
                }
            }

            inscription.PrecioTotal = totalPrice;

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            _context.Add(inscription);
            await _context.SaveChangesAsync();

            var resultDetail = new ClaseDeportivaInscripcionDetailDTO(
                inscription.Id,
                inscription.FechaInscripcion,
                inscription.NombreUsuario,
                inscription.ApellidosUsuario,
                inscription.DNI,
                inscription.Telefono,
                inscription.DatosPago,
                inscripcionForCreate.MetodoPago,
                inscription.PrecioTotal,
                inscription.ClasesInscritas.Select(ci => {
                    var sc = classes.First(c => c.Id == ci.ClaseDeportivaId);
                    return new ClassInscriptionItemDTO(
                        sc.Id,
                        sc.Descripcion,
                        sc.TipoDeporte.Nombre,
                        sc.FechaHora,
                        ci.PlazasReservadas,
                        ci.Precio,
                        ci.Observaciones
                    );
                }).ToList()
            );

            return CreatedAtAction(nameof(GetInscripcionClaseDeportiva), new { id = inscription.Id }, resultDetail);
        }
    }
}
