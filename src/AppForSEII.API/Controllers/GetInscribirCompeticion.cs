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
            IList<DTOInscribirseCompeticion> selectCompetitions = await _context.Competitions
                .Include(c => c.SportType)
                .Include(c => c.Inscriptions)
                .Where(c => 
                    (competitionName == null || c.Name.Contains(competitionName))
                    && (sportType == null || c.SportType.Name.Equals(sportType))
                    && (location == null || c.Location.Equals(location))
                    && (c.Inscriptions.Count < c.MaxCapacity)
                )
                .OrderBy(c => c.Date)
                .Select(c => new DTOInscribirseCompeticion(
                    c.Id,
                    c.Name,
                    c.SportType.Name,
                    c.Date,
                    c.Location,
                    c.Price
                ))
                .ToListAsync();

            return Ok(selectCompetitions);
        }
    }
}