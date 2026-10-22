using AppForSEII.API.Data;
using AppForSEII.API.Models;
using AppForSEII.Shared.CompetitionDTOs;
using Microsoft.AspNetCore.Http;
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
    public class GetInscribirCompeticion : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetInscribirCompeticion> _logger;

        public GetInscribirCompeticion(ApplicationDbContext context, ILogger<GetInscribirCompeticion> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- GET de Selección (Paso 2) ---
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DTOInscribirseCompeticion>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetCompetitionsForRegistration(string? competitionName, string? sportType, string? location)
        {
            IList<DTOInscribirseCompeticion> selectCompetitions = await _context.Competiciones
                .Include(c => c.TipoDeporte)
                .Include(c => c.CompeticionesInscritas)
                .Where(c => 
                    (competitionName == null || c.Nombre.Contains(competitionName))
                    && (sportType == null || c.TipoDeporte.Nombre.Equals(sportType))
                    && (location == null || c.Lugar.Equals(location))
                    && (c.CompeticionesInscritas.Count < c.Plazas)
                )
                .OrderBy(c => c.Fecha)
                .Select(c => new DTOInscribirseCompeticion(
                    c.Id,
                    c.Nombre,
                    c.TipoDeporte.Nombre,
                    c.Fecha,
                    c.Lugar,
                    (double)c.Precio
                ))
                .ToListAsync();

            return Ok(selectCompetitions);
        }

        // --- GET Detalle de Inscripción (Equivalente a GetRental) ---
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(CompeticionInscripcionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetCompeticionInscripcion(int id)
        {
            var inscription = await _context.Inscripciones
                .Where(i => i.Id == id)
                .Include(i => i.CompeticionesInscritas)
                    .ThenInclude(ci => ci.Competicion)
                        .ThenInclude(c => c.TipoDeporte)
                .Select(i => new CompeticionInscripcionDetailDTO(
                    i.Id,
                    i.FechaInscripcion,
                    i.NombreUsuario,
                    i.ApellidosUsuario,
                    i.DNI,
                    i.Telefono,
                    i.DatosPago,
                    (MetodoPagoTypes)i.MetodoPago,
                    i.PrecioTotal,
                    i.CompeticionesInscritas.Select(ci => new CompeticionInscritaItemDTO(
                        ci.Competicion.Id,
                        ci.Competicion.Nombre,
                        ci.Competicion.TipoDeporte.Nombre,
                        ci.Competicion.Fecha,
                        ci.Competicion.Lugar,
                        ci.Competicion.Precio,
                        ci.ProblemasFisicos
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (inscription == null)
            {
                _logger.LogError($"Error: Inscripción con id {id} no existe");
                return NotFound();
            }

            return Ok(inscription);
        }

        // --- POST Creación de Inscripción (Equivalente a CreateRental) ---
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(CompeticionInscripcionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateCompeticionInscripcion(CompeticionInscripcionForCreateDTO inscriptionForCreate)
        {
            if (inscriptionForCreate.CompeticionesInscritas.Count == 0)
            {
                ModelState.AddModelError("CompeticionesInscritas", "Error! Debe incluir al menos una competición a la que inscribirse.");
            }

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var competicionIds = inscriptionForCreate.CompeticionesInscritas.Select(ci => ci.CompeticionId).ToList();

            var competiciones = await _context.Competiciones
                .Include(c => c.TipoDeporte)
                .Include(c => c.CompeticionesInscritas)
                .Where(c => competicionIds.Contains(c.Id))
                .ToListAsync();

            var inscription = new Inscripcion(
                inscriptionForCreate.NombreUsuario,
                inscriptionForCreate.ApellidosUsuario,
                inscriptionForCreate.Dni,
                inscriptionForCreate.Telefono,
                DateTime.Now,
                (MetodoPago)(int)inscriptionForCreate.MetodoPago,
                0,
                inscriptionForCreate.DatosPago
            );

            inscription.CompeticionesInscritas = new List<CompeticionInscrita>();
            decimal totalPrice = 0;

            foreach (var item in inscriptionForCreate.CompeticionesInscritas)
{
    var competicion = competiciones.FirstOrDefault(c => c.Id == item.CompeticionId);

    if (competicion == null || competicion.CompeticionesInscritas.Count >= competicion.Plazas)
    {
        ModelState.AddModelError("CompeticionesInscritas", $"Error! La competición id '{item.CompeticionId}' no existe o no tiene plazas libres disponibles.");
    }
    else
    {
        inscription.CompeticionesInscritas.Add(new CompeticionInscrita(
            item.CompeticionId, 
            inscription.Id, 
            item.ProblemasFisicos
        ));
        
        totalPrice += competicion.Precio;
    }
}

            inscription.PrecioTotal = totalPrice;

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            _context.Add(inscription);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Conflict("Error: " + ex.Message);
            }

            var resultDetail = new CompeticionInscripcionDetailDTO(
                inscription.Id,
                inscription.FechaInscripcion,
                inscription.NombreUsuario,
                inscription.ApellidosUsuario,
                inscription.DNI,
                inscription.Telefono,
                inscription.DatosPago,
                inscriptionForCreate.MetodoPago,
                inscription.PrecioTotal,
                inscription.CompeticionesInscritas.Select(ci => {
                    var comp = competiciones.First(c => c.Id == ci.CompeticionId);
                    return new CompeticionInscritaItemDTO(
                        comp.Id,
                        comp.Nombre,
                        comp.TipoDeporte.Nombre,
                        comp.Fecha,
                        comp.Lugar,
                        comp.Precio,
                        ci.ProblemasFisicos
                    );
                }).ToList()
            );

            return CreatedAtAction("GetCompeticionInscripcion", new { id = inscription.Id }, resultDetail);
        }
    }
}