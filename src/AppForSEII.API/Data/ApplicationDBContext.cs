using AppForSEII.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.DTOs.ApplicationUserDTO;

namespace AppForSEII.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {

        base.OnModelCreating(builder);


    }


    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Inscripcion> Inscripciones { get; set; }
    public DbSet<Pista> Pistas { get; set; } // Asignación explícita de las clases que tendrá la base de datos

    public DbSet<PistaReservada> PistasReservadas { get; set; } // DbSet para la entidad intermedia PistaReservada
    public DbSet<TipoDeporte> TiposDeportes { get; set; }

    public DbSet<ClaseInscrita> ClasesInscritas { get; set; }

    public DbSet<ClaseDeportiva> ClasesDeportivas { get; set; }

    public DbSet<Competicion> Competiciones { get; set; }

    public DbSet<CompeticionInscrita> CompeticionesInscritas { get; set; }



}