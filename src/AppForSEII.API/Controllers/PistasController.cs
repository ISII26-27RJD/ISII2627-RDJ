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
        
    }//De la clase controlador
}//Del namespace