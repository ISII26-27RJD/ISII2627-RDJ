using AppForSEII.API.Data;
using AppForSEII.Shared.ClassDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetInscribirClasesDeportivas : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetInscribirClasesDeportivas> _logger;

        public GetInscribirClasesDeportivas(ApplicationDbContext context, ILogger<GetInscribirClasesDeportivas> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DTOInscribirseClasesDeportivas>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetSportClassesForRegistration(string? sportType, string? monitor, DateTime? date)
        {
            IList<DTOInscribirseClasesDeportivas> selectClasses = await _context.ClasesDeportivas
                .Include(c => c.TipoDeporte)
                .Where(c => 
                    (sportType == null || c.TipoDeporte.Nombre.Equals(sportType))
                    && (monitor == null || c.Monitor.Contains(monitor))
                    && (date == null || c.FechaHora.Date.Equals(date.Value.Date))
                    && (c.PlazasDisponibles > 0)
                )
                .OrderBy(c => c.FechaHora)
                .Select(c => new DTOInscribirseClasesDeportivas(
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
    }
}