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
        //[HttpGet]
        //[Route("[action]")]
        [HttpGet("[action]")] // Se cambio a esta forma por detección del fallo 500
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

        //GET de los detalles de las pistas
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReservaDetailsDTO), (int)System.Net.HttpStatusCode.OK)]
        [ProducesResponseType((int)System.Net.HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetReservaDetails(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente) // Unimos con el usuario para sacar su DNI, Nombre, etc.
                .Include(r => r.PistasReservadas) // Unimos con la tabla intermedia
                    .ThenInclude(pr => pr.Pista) // Unimos con la pista física
                        .ThenInclude(p => p.TipoDeporte) // Unimos con el deporte para el nombre
                .Where(r => r.Id == id)
                .Select(r => new ReservaDetailsDTO(
                    r.Id, 
                    r.FechaReserva, 
                    r.MetodoPago, 
                    "****-****-****-****", //Esto es un parche visual para los detalles del pago.
                    // Transformamos las PistasReservadas de la BD en DTOs usando LINQ
                    r.PistasReservadas.Select(pr => new PistaReservadaForCreateItemDTO(
                        pr.IdPista, 
                        pr.Observaciones, 
                        pr.Pista.NombrePista, 
                        pr.Pista.TipoDeporte.Nombre, 
                        pr.Pista.NPersonas, 
                        pr.Precio
                    )).ToList(),
                    r.Cliente.DNI, 
                    r.Cliente.Name, 
                    r.Cliente.Surname, 
                    r.Cliente.Email, 
                    r.Cliente.PhoneNumber ?? string.Empty
                ))
                .FirstOrDefaultAsync();

            if (reserva == null)
            {
                return NotFound();
            }

            return Ok(reserva);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        [HttpPost]
        [Route("[action]")]
        // De momento no sé como devolver un DTO especifico, así que para la gracias de las pruebas, solo decimos que devolvemos un 201 Created
        // Ojo, pienso que esto no será algo definitivo, por lo que la respuesta que mande será distinta.
        // Actualizamos el ProducesResponseType para que prometa devolver el ticket detallado
        [ProducesResponseType(typeof(ReservaDetailsDTO), (int)System.Net.HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)System.Net.HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)System.Net.HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreatePistaReserva(ReservaForCreateDTO reservaForCreate)
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

            // Calculamos la fecha objetivo (Si viene vacía, usamos Hoy)
            DateTime fechaObjetivo = reservaForCreate.FechaReserva ?? DateTime.Today;

            // Crear la entidad principal (Reserva)
            var nuevaReserva = new Reserva(
                clienteId: currentUser.Id,
                fechaReserva: fechaObjetivo, 
                metodoPago: reservaForCreate.PaymentMethod,
                precioTotal: 0m 
            );

            decimal precioTotalCalculado = 0;

            // Vamos a procesar el carro de la compra
            foreach (var itemDTO in reservaForCreate.PistasReservadas)
            {
                // Añadimos el Include para poder sacar NombrePista y TipoDeporte
                var pistaReal = await _context.Pistas
                    .Include(p => p.TipoDeporte)
                    .FirstOrDefaultAsync(p => p.IdPista == itemDTO.IdPista);
                
                if (pistaReal == null)
                {
                    ModelState.AddModelError("PistaNoEncontrada", $"La pista con ID {itemDTO.IdPista} no existe.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                // 2. Comprobamos el stock PARA LA FECHA OBJETIVO, no para DateTime.Today
                var ocupadasEseDia = await _context.PistasReservadas
                    .Where(pr => pr.IdPista == pistaReal.IdPista && pr.Reserva.FechaReserva.Date == fechaObjetivo.Date)
                    .SumAsync(pr => pr.Cantidad);

                if (ocupadasEseDia + 1 > pistaReal.Stock)
                {
                    ModelState.AddModelError("SinStock", $"La pista '{pistaReal.NombrePista}' ya está agotada para el día {fechaObjetivo.ToShortDateString()}.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                var pistaReservada = new PistaReservada
                {
                    Cantidad = 1,
                    Precio = pistaReal.Precio,
                    Observaciones = itemDTO.Observaciones,
                    IdPista = pistaReal.IdPista,
                    //EF Core usa estos objetos para rellenar las claves foráneas fantasma (PistaIdPista) en SQL Server.
                    Pista = pistaReal,
                    Reserva = nuevaReserva
                    //NO paso id para la reserva, porque EF Core solucionará por si sola clave foranea.
                };

                nuevaReserva.PistasReservadas.Add(pistaReservada);
                precioTotalCalculado += pistaReal.Precio;
                
                // Respecto a DETAILS: Rellenamos el DTO de entrada con los datos reales de la BD
                // para que luego el ticket se imprima con el desglose pedido
                itemDTO.NombrePista = pistaReal.NombrePista;
                itemDTO.TipoDeporte = pistaReal.TipoDeporte.Nombre;
                itemDTO.NPersonas = pistaReal.NPersonas;
                itemDTO.Precio = pistaReal.Precio;
            }//Del for-each

            // Asignar el precio final a prueba de manipulaciones
            nuevaReserva.PrecioTotal = precioTotalCalculado;

            // Guardar en base de datos
            _context.Reservas.Add(nuevaReserva);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Extraemos el error real de SQL Server que está oculto en el InnerException
                var errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("Reserva", $"Error! Hubo un problema al guardar su reserva.");
                return Conflict("Error SQL: " + errorReal);
            }

            // 7. Preparar DTO de Detalles (El Ticket de Compra)
            var reservaDetails = new ReservaDetailsDTO(
                id: nuevaReserva.Id,
                fechaReserva: nuevaReserva.FechaReserva,
                metodoPago: reservaForCreate.PaymentMethod,
                datosPago: "****-****-****-****", // <- Ocultamos el dato al devolver el recibo
                pistasReservadas: reservaForCreate.PistasReservadas,
                clienteDNI: currentUser.DNI, 
                clienteNombre: currentUser.Name,
                clienteApellidos: currentUser.Surname,
                clienteEmail: currentUser.Email,
                clienteTelefono: currentUser.PhoneNumber ?? string.Empty
            );

            // Retornar 201 CreatedAtAction
            // Como aún no tenemos un método GET para reservas, puedes usar string.Empty o 
            // inventar el nombre del futuro método "GetReservaDetails"
            return CreatedAtAction("GetReservaDetails", new { id = nuevaReserva.Id }, reservaDetails);
            
        }//Del método POST

    }//De la clase controlador

}//Del namespace