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
    // Controlador para el caso de uso "Inscribirse a Clase Deportiva".
    // Gestiona tres operaciones: SELECT (catalogo), GET DETAIL (detalle) y POST (crear inscripcion).
    [Route("api/[controller]")]
    [ApiController]
    public class GETClaseDeportivaParaInscribirse : ControllerBase
    {
        // Acceso a la base de datos mediante el contexto de Entity Framework.
        private readonly ApplicationDbContext _context;
        // Logger para registrar errores y trazas.
        private readonly ILogger<GETClaseDeportivaParaInscribirse> _logger;

        // Constructor: EF Core y el logger se inyectan automaticamente desde Program.cs.
        public GETClaseDeportivaParaInscribirse(ApplicationDbContext context, ILogger<GETClaseDeportivaParaInscribirse> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =============================================================
        // SELECT  -  GET catalogo de clases deportivas disponibles
        // =============================================================

        // Devuelve el catalogo de clases deportivas disponibles (con plazas > 0), con filtros opcionales.
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<ClaseDeportivaParaInscribirseDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetClasesDeportivasParaInscribirse(
            string? filtroNombre,        // Filtra por descripcion de la clase (busqueda parcial con Contains)
            string? filtroTipoDeporte,   // Filtra por nombre exacto del tipo de deporte
            DateTime? fromDate,          // Solo clases a partir de esta fecha
            DateTime? toDate)            // Solo clases hasta esta fecha
        {
            // Validacion: si se indican ambas fechas, fromDate debe ser anterior a toDate.
            if (fromDate != null && toDate != null && fromDate.Value > toDate.Value)
            {
                ModelState.AddModelError("fromDate", "fromDate must be earlier than toDate");
                _logger.LogError($"{DateTime.Now} Error: fromDate must be earlier than toDate");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Construccion dinamica de la consulta con Include para traer el TipoDeporte relacionado.
            // Cada filtro solo se aplica si su valor no es null.
            var query = _context.ClasesDeportivas
                .Include(c => c.TipoDeporte)
                .Where(c => 
                    (filtroNombre == null || c.Descripcion.Contains(filtroNombre))
                    && (filtroTipoDeporte == null || c.TipoDeporte.Nombre.Equals(filtroTipoDeporte))
                    && (fromDate == null || c.FechaHora.Date >= fromDate.Value.Date)
                    && (toDate == null || c.FechaHora.Date <= toDate.Value.Date)
                    && (c.PlazasDisponibles > 0)  // Solo se muestran clases con plazas disponibles
                )
                .OrderBy(c => c.Descripcion);  // Resultado ordenado alfabeticamente

            // Proyeccion: se seleccionan solo los campos necesarios para el DTO (no toda la entidad).
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

        // =============================================================
        // GET DETAIL  -  Detalle de una inscripcion existente
        // =============================================================

        // Devuelve el detalle completo de una inscripcion concreta, incluyendo sus clases inscritas.
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ClaseDeportivaInscripcionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetInscripcionClaseDeportiva(int id)
        {
            // Consulta con Include encadenado para cargar la inscripcion, sus clases inscritas,
            // la clase deportiva de cada una y el tipo de deporte de cada clase.
            var inscription = await _context.Inscripciones
                .Where(i => i.Id == id)
                .Include(i => i.ClasesInscritas)
                    .ThenInclude(ci => ci.ClaseDeportiva)
                        .ThenInclude(cd => cd.TipoDeporte)
                // Proyeccion al DTO de detalle, mapeando tambien cada linea de ClaseInscrita a ClassInscriptionItemDTO.
                .Select(i => new ClaseDeportivaInscripcionDetailDTO(
                    i.Id,
                    i.FechaInscripcion,
                    i.NombreUsuario,
                    i.ApellidosUsuario,
                    i.DNI,
                    i.Telefono,
                    i.DatosPago,
                    (MetodoPagoTypes)i.MetodoPago,  // Cast del enum de dominio al enum del shared DTO
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

            // Si no existe ninguna inscripcion con ese Id, devuelve 404.
            if (inscription == null)
            {
                _logger.LogError($"Error: Inscripcion con id {id} no existe");
                return NotFound();
            }

            return Ok(inscription);
        }

        // =============================================================
        // POST  -  Crear una nueva inscripcion a clase(s) deportiva(s)
        // =============================================================

        // Crea una nueva inscripcion a una o varias clases deportivas.
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ClaseDeportivaInscripcionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CreateInscripcionClaseDeportiva(ClaseDeportivaInscripcionForCreateDTO inscripcionForCreate)
        {
            // Validacion: debe incluirse al menos una clase deportiva en la inscripcion.
            if (inscripcionForCreate.ClasesInscritas == null || inscripcionForCreate.ClasesInscritas.Count == 0)
            {
                ModelState.AddModelError("ClasesInscritas", "Error! Debes incluir al menos una clase deportiva para inscribirte");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Busca el usuario por UserName o por DNI para vincularlo a la inscripcion.
            var user = await _context.Users
                .OfType<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.UserName == inscripcionForCreate.NombreUsuario || u.DNI == inscripcionForCreate.Dni);

            // Si el usuario no existe, se registra el error (pero NO se devuelve aun — se acumulan los errores).
            if (user == null)
            {
                ModelState.AddModelError("NombreUsuario", "Error! El nombre de usuario no esta registrado");
            }

            // Obtiene de BD todas las clases deportivas solicitadas en una sola consulta (evita N+1 queries).
            var claseIds = inscripcionForCreate.ClasesInscritas.Select(ci => ci.ClaseDeportivaId).ToList();
            var classes = await _context.ClasesDeportivas
                .Include(c => c.TipoDeporte)
                .Where(c => claseIds.Contains(c.Id))
                .ToListAsync();

            // Crea el objeto Inscripcion con los datos del DTO. PrecioTotal empieza en 0 y se acumula.
            var inscription = new Inscripcion(
                inscripcionForCreate.NombreUsuario,
                inscripcionForCreate.ApellidosUsuario,
                inscripcionForCreate.Dni,
                inscripcionForCreate.Telefono,
                DateTime.Now,
                (MetodoPago)(int)inscripcionForCreate.MetodoPago,  // Cast del enum shared al enum de dominio
                0,  // PrecioTotal se calculara a continuacion
                inscripcionForCreate.DatosPago
            );

            // Si se encontro el usuario, se vincula como Cliente de la inscripcion (FK de navegacion).
            if (user != null)
            {
                inscription.Cliente = user;
            }

            inscription.ClasesInscritas = new List<ClaseInscrita>();
            decimal totalPrice = 0;

            // Procesa cada clase solicitada: valida disponibilidad, crea la ClaseInscrita y descuenta plazas.
            foreach (var item in inscripcionForCreate.ClasesInscritas)
            {
                var sportClass = classes.FirstOrDefault(c => c.Id == item.ClaseDeportivaId);

                // Si la clase no existe en BD o no tiene suficientes plazas, se agrega un error de validacion.
                if (sportClass == null || sportClass.PlazasDisponibles < item.PlazasReservadas)
                {
                    ModelState.AddModelError("ClasesInscritas", $"Error! La clase deportiva '{item.ClaseDeportivaId}' no esta disponible para inscribirse");
                }
                else
                {
                    // Calcula el precio de esta linea: precio unitario x plazas reservadas.
                    decimal itemTotal = sportClass.PrecioUnitario * item.PlazasReservadas;
                    
                    // Crea la entidad ClaseInscrita (la tabla intermedia entre Inscripcion y ClaseDeportiva).
                    inscription.ClasesInscritas.Add(new ClaseInscrita(
                        sportClass.Id, 
                        inscription.Id, 
                        item.PlazasReservadas, 
                        itemTotal, 
                        item.Observaciones
                    ));

                    // Descuenta las plazas reservadas de las disponibles en la clase.
                    sportClass.PlazasDisponibles -= item.PlazasReservadas;
                    // Acumula el precio total de la inscripcion.
                    totalPrice += itemTotal;
                }
            }

            // Asigna el precio total calculado a la inscripcion.
            inscription.PrecioTotal = totalPrice;

            // Si hay algun error acumulado (usuario no existe, clase no disponible...), devuelve 400.
            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Persiste la inscripcion (y sus ClasesInscritas en cascada) en la BD.
            _context.Add(inscription);
            await _context.SaveChangesAsync();

            // Construye el DTO de detalle para devolverlo en la respuesta 201 Created.
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

            // Devuelve 201 Created con la cabecera Location apuntando al GET detail de la inscripcion creada.
            return CreatedAtAction(nameof(GetInscripcionClaseDeportiva), new { id = inscription.Id }, resultDetail);
        }
    }
}
