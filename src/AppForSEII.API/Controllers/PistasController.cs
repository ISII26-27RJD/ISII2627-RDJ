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
                
            return NotFound();
        }//Del método de details

    }//De la clase controlador

}//Del namespace