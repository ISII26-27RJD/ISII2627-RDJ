using AppForSEII.API.Data;
using AppForSEII.Shared.CompetitionDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

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
    }
}