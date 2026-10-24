using AppForSEII.API.Data;
using AppForSEII.API.Models;
using AppForSEII.Shared.ClassDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportClassesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SportClassesController> _logger;

        public SportClassesController(ApplicationDbContext context, ILogger<SportClassesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET Selección 
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

        //GET Detalle de Inscripción
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ClassInscriptionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetClassInscription(int id)
        {
            var inscription = await _context.Inscripciones
                .Where(i => i.Id == id)
                .Include(i => i.ClasesInscritas)
                    .ThenInclude(ci => ci.ClaseDeportiva)
                        .ThenInclude(cd => cd.TipoDeporte)
                .Select(i => new ClassInscriptionDetailDTO(
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
                _logger.LogError($"Error: Inscripción a clase con id {id} no existe");
                return NotFound();
            }

            return Ok(inscription);
        }

        //POST Crear Inscripción
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ClassInscriptionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateClassInscription(ClassInscriptionForCreateDTO inscriptionForCreate)
        {
            if (inscriptionForCreate.ClasesInscritas.Count == 0)
            {
                ModelState.AddModelError("ClasesInscritas", "Error! Debe incluir al menos una clase deportiva a la que apuntarse.");
            }

            var user = _context.ApplicationUsers.FirstOrDefault(au => au.DNI == inscriptionForCreate.Dni || au.UserName == inscriptionForCreate.NombreUsuario);

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var claseIds = inscriptionForCreate.ClasesInscritas.Select(ci => ci.ClaseDeportivaId).ToList();

            var classes = await _context.ClasesDeportivas
                .Include(c => c.TipoDeporte)
                .Where(c => claseIds.Contains(c.Id))
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

            if (user != null)
            {
                inscription.Cliente = user;
            }

            inscription.ClasesInscritas = new List<ClaseInscrita>();
            decimal totalPrice = 0;

            foreach (var item in inscriptionForCreate.ClasesInscritas)
            {
                var sportClass = classes.FirstOrDefault(c => c.Id == item.ClaseDeportivaId);

                // comprobamos que existe y que haya plazas disponibles
                if (sportClass == null || sportClass.PlazasDisponibles < item.PlazasReservadas)
                {
                    ModelState.AddModelError("ClasesInscritas", $"Error! La clase deportiva con id '{item.ClaseDeportivaId}' no existe o no tiene suficientes plazas disponibles.");
                }
                else
                {
                    decimal itemTotal = sportClass.PrecioUnitario * item.PlazasReservadas;

                    // creamos la relacion usando el constructor de ClaseInscrita
                    inscription.ClasesInscritas.Add(new ClaseInscrita(
                        sportClass.Id, 
                        inscription.Id, 
                        item.PlazasReservadas, 
                        itemTotal, 
                        item.Observaciones
                    ));

                    // descontamos las plazas reservadas de la clase
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("Inscripcion", "Error! Hubo un problema al guardar su inscripción, por favor inténtelo de nuevo más tarde.");
                return Conflict("Error: " + ex.Message);
            }

            var resultDetail = new ClassInscriptionDetailDTO(
                inscription.Id,
                inscription.FechaInscripcion,
                inscription.NombreUsuario,
                inscription.ApellidosUsuario,
                inscription.DNI,
                inscription.Telefono,
                inscription.DatosPago,
                inscriptionForCreate.MetodoPago,
                inscription.PrecioTotal,
                inscription.ClasesInscritas.Select(ci => {
                    var sc = classes.First(c => c.Id == ci.ClaseDeportivaId);
                    return new ClassInscriptionItemDTO(
                        sc.Id,
                        sc.Descripcion,
                        sc.TipoDeporte?.Nombre ?? "",
                        sc.FechaHora,
                        ci.PlazasReservadas,
                        ci.Precio,
                        ci.Observaciones
                    );
                }).ToList()
            );

            return CreatedAtAction("GetClassInscription", new { id = inscription.Id }, resultDetail);
        }
    }
}