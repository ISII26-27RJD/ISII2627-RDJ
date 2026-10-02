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
<<<<<<< HEAD

=======
    public DbSet<PistaReservada> PistasReservadas { get; set; } // DbSet para la entidad intermedia PistaReservada
>>>>>>> f99dde720bf2cddf03df74a2868cc3c3cab18d6c
    public DbSet<TipoDeporte> TiposDeportes { get; set; }

    public DbSet<ClaseInscrita> ClasesInscritas { get; set; }

    public DbSet<ClaseDeportiva> ClasesDeportivas { get; set; }


}