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
            return Ok("Todo en orden"); //Esta línea NO es permanente, es solo para que deje de saltar error en código.
        }

    }//De la clase controlador
}//Del namespace