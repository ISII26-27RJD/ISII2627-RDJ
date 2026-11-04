using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppForSEII.API.Data;
using AppForSEII.API.Models;
using AppForSEII.API.DTOs.PistasDTOs;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")] //URL exacta que el cliente tiene que escribir para llegar al código
    [ApiController] //No soy un controlador tradicional, solo escupo datos
    public class PistasController : ControllerBase
    {
        private readonly ApplicationDbContext _context; //La base de datos
        private readonly ILogger<PistasController> _logger; //Deja registro de lo que pase

        //Dependencias, framework nos deja contexto y logger automáticamente
        public PistasController(ApplicationDbContext context, ILogger<PistasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //GET de seleccionar pistas para reserva
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<PistaParaReservarDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetPistasParaReservar(string? nombreDeporte, DateTime? fechaReserva)
        {
            //Filtro. Aseguro que no reservo pistas para jugar "ayer". No interesa ver pistas alquiladas la semana pasada
            if (fechaReserva != null && fechaReserva.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("fechaReserva", "La fecha de reserva no puede ser anterior al día de hoy.");
                _logger.LogError($"{DateTime.Now} Error: Intento de buscar pistas en una fecha pasada ({fechaReserva}).");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            //¿No paso fecha? No pasa nada, usa la de hoy
            DateTime fechaFiltro = fechaReserva ?? DateTime.Today;

            //CONSULTA A LA BASE DE DATOS
            var pistasDisponibles = await _context.Pistas
            .Include(p => p.TipoDeporte) //Obten nombre deporte
            .Include(p => p.PistasReservadas).ThenInclude(pr => pr.Reserva) //Miramos pistasReservadas, luego en Reserva para ver stock
            //Filtros de flujos alternativos
            .Where(p =>
                //Primer filtro, si metemos deporte, asegurar que coincide con el contexto
                (string.IsNullOrEmpty(nombreDeporte) || p.TipoDeporte.Nombre == nombreDeporte) 
                &&
                //Segundo filtro: validar stock. Vamos a ver cuantas reservas tiene una pista en concreto, en un día especifico, y verificamos si esto es menor que la realidad.
                (p.PistasReservadas
                    .Where(pr => pr.Reserva.FechaReserva.Date == fechaFiltro.Date)
                    .Sum(pr => (int?)pr.Cantidad) ?? 0) < p.Stock
            )//Del where
            .OrderBy(p => p.NombrePista) //Ordenamos alfabeticamente por nombres
            //Aplanamos entidades
            .Select(p => new PistaParaReservarDTO(
                p.IdPista, 
                p.NombrePista, 
                p.NPersonas, 
                p.TipoDeporte.Nombre, 
                p.Precio
            ))
            .ToListAsync();
            
            // 5. Retornamos la respuesta (Código 200 OK)
            return Ok(pistasDisponibles);

        }// Del metodo de SELECT

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        //GET delos detalles de las pistas
        [HttpGet]
        [Route("[action]/{id}")] //nombre del método
        [ProducesResponseType(typeof(PistaReservaDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetPistaDetails(int id)
        {
            //Consulta de stock
            var pistaDetails = await _context.Pistas
                
                // Cargamos la relación para poder aplanar el nombre del deporte
                .Include(p => p.TipoDeporte)
                
                // Filtramos por la Clave Primaria que viene en la URL
                .Where(p => p.IdPista == id)
                
                // Proyección al DTO
                .Select(p => new PistaReservaDetailsDTO(
                    p.IdPista,
                    p.NombrePista,
                    p.NPersonas,
                    p.TipoDeporte.Nombre,
                    p.Precio
                ))
                
                // Ejecuta la consulta y devuelve el primer resultado, o null si no existe
                .FirstOrDefaultAsync();

            //¿Que pasa si la pista fuera null, es decir, que no existe, al menos en la base de datos?
            if (pistaDetails == null)
            {
                // Dejamos registro en el servidor de que alguien o algo buscó una pista inválida
                _logger.LogError($"{DateTime.Now} Error: Intento de acceder a detalles de una pista inexistente (ID: {id}).");
                
                // Cumplimos con REST devolviendo el código correcto para "Recurso no encontrado"
                return NotFound();
            }

            return Ok(pistaDetails);
            
        }//Del método de details

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        [HttpPost]
        [Route("[action]")]
        // De momento no sé como devolver un DTO especifico, así que para la gracias de las pruebas, solo decimos que devolvemos un 201 Created
        // Ojo, pienso que esto no será algo definitivo, por lo que la respuesta que mande será distinta.
        [ProducesResponseType((int)System.Net.HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)System.Net.HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CreateReserva(ReservaForCreateDTO reservaForCreate)
        {
            //Namespace de DTOS para POST corregidos

            //Necesitamos pasar ciertos filtros antes de tocar nada
            if (_context.Reservas == null) //¿¿La tabla de reservas ha sido insertada en la basede datos???
            {
                return Problem("Entity set 'ApplicationDbContext.Reservas' is null.");
            }

            if (!ModelState.IsValid)
            {
                //¿Que es modelstate? Un diccionario interno de ASP.NET Core. Cuando JSON del frontend llega al servidor, framework intenta encajarlo con el DTO de post.
                // Leemos en el proceso todas las reglas ya establecidas (required, minlength, etc). Si alguna regla no se cumple, entonces la bandera se pone en falso
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Simulación de usuario autenticado.
            // Buscamos a Peter para simular que es él quien hace la petición POST
            var currentUser = await _context.Users.OfType<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.UserName == "peter@uclm.es");

            if (currentUser == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            // 3. Crear la entidad principal (Reserva)
            var nuevaReserva = new Reserva(
                clienteId: currentUser.Id,
                fechaReserva: DateTime.Today, // El CU asume que la reserva es para hoy
                metodoPago: reservaForCreate.MetodoPago,
                precioTotal: 0m // Lo calcularemos en el siguiente paso de forma segura
            );

            decimal precioTotalCalculado = 0;

            // 4. Procesar el carrito (Desglose de pistas)
            foreach (var itemDTO in reservaForCreate.PistasReservadas)
            {
                //Buscar la pista real en base de datos para obtener su precio y stock verificados
                var pistaReal = await _context.Pistas.FirstOrDefaultAsync(p => p.IdPista == itemDTO.IdPista);
                
                if (pistaReal == null)
                {
                    ModelState.AddModelError("PistaNoEncontrada", $"La pista con ID {itemDTO.IdPista} no existe.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                // Verificar disponibilidad (¿tengo suficientes pistas como para que hoy se pueda reservar?)
                var ocupadasHoy = await _context.PistasReservadas
                    .Where(pr => pr.IdPista == pistaReal.IdPista && pr.Reserva.FechaReserva.Date == DateTime.Today)
                    .SumAsync(pr => pr.Cantidad);

                if (ocupadasHoy + 1 > pistaReal.Stock) 
                {
                    // Si las pistas que de momento tenemos reservadas + 1, supera la cantidad real de stock que tenemos a mano.....
                    // +1 porque estamos intentando reservar 1 cantidad
                    ModelState.AddModelError("SinStock", $"La pista '{pistaReal.NombrePista}' ya está agotada para hoy.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                // Generamos la pista reservada si tenemos stock
                var pistaReservada = new PistaReservada
                {
                    Cantidad = 1,
                    Precio = pistaReal.Precio, // Precio real de la BD, no del Frontend
                    Observaciones = itemDTO.Observaciones,
                    IdPista = pistaReal.IdPista,
                    Reserva = nuevaReserva
                };

                nuevaReserva.PistasReservadas.Add(pistaReservada); //Añadimos a la lista de reservas
                precioTotalCalculado += pistaReal.Precio; 
            }//Del for-each

            // Asignar el precio final a prueba de manipulaciones
            nuevaReserva.PrecioTotal = precioTotalCalculado;

            // Guardar en Base de Datos (Transacción atómica de EF Core)
            _context.Reservas.Add(nuevaReserva);
            await _context.SaveChangesAsync();

            // 7. Retornar 201 Created (Minimalista)
            // Devolvemos un objeto anónimo solo con el ID. Si en el futuro tienemos un método GET 
            // para ver los detalles de una reserva (Lista de detalles), cambia string.Empty por el nombre de ese método.
            //(posiblemente necesario un quinto DTO)
            return Created(string.Empty, new { 
                mensaje = "Reserva procesada correctamente", 
                idReserva = nuevaReserva.Id
            });
            
        }//Del método POST

    }//De la clase controlador

}//Del namespace