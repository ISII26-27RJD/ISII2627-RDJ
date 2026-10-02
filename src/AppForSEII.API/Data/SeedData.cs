using System;
using System.Collections.Generic;
using System.Linq;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppForSEII.API.Data 
{
    public class SeedData 
    {
        public static void Initialize(ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger logger) 
        {
            List<string> rolesNames = new List<string> { "Administrator", "Employee", "Customer" };

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            try {
                SeedRoles(roleManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the roles in the Database.");
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            try {
                SeedUsers(userManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Users in the Database.");
            }

            try {
                //it initializes the database with tipos de deporte, competiciones and clases deportivas
                SeedTiposDeporteCompeticionesYClases(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Sports and Classes in the Database.");
            }

            try {
                var user = dbContext.Users.OfType<ApplicationUser>().FirstOrDefault(u => u.UserName == "peter@uclm.es");

                //it initializes the database with an Inscripcion
                SeedInscripciones(dbContext, user);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding an Inscripcion in the Database.");
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager, List<string> roles) 
        {
            foreach (string roleName in roles) {
                //it checks such role does not exist in the database 
                if (!roleManager.RoleExistsAsync(roleName).Result) {
                    IdentityRole role = new IdentityRole();
                    role.Name = roleName;
                    role.NormalizedName = roleName;
                    IdentityResult roleResult = roleManager.CreateAsync(role).Result;
                }
            }
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager, List<string> roles) 
        {
            //first, it checks the user does not already exist in the DB
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es", "11111111A", 45, "Femenino");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "Password1234%");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    //administrator role
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }

            if (userManager.FindByNameAsync("gregorio@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("2", "Gregorio", "Diaz Descalzo", "gregorio@uclm.es", "22222222B", 50, "Masculino");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "APassword1234%");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    //employee role
                    userManager.AddToRoleAsync(user, roles[1]).Wait();
                }
            }

            if (userManager.FindByNameAsync("peter@uclm.es").Result == null) {
                //A customer class has been defined because it has different attributes
                ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es", "33333333C", 30, "Masculino");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "OtherPass12$");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    //customer role
                    userManager.AddToRoleAsync(user, roles[2]).Wait();
                }
            }
        }

        public static void SeedTiposDeporteCompeticionesYClases(ApplicationDbContext dbContext) 
        {
            string[] nombresDeportes = ["Pádel", "Tenis", "Baloncesto", "Fútbol 7"];
            List<TipoDeporte> tiposDeporte = new List<TipoDeporte>();

            var tiposDeportesSet = dbContext.Set<TipoDeporte>();
            foreach (string nombreDeporte in nombresDeportes) {
                var tipo = tiposDeportesSet.FirstOrDefault(t => t.Nombre == nombreDeporte);
                if (tipo == null) {
                    tipo = new TipoDeporte(nombreDeporte, "Deporte de " + nombreDeporte);
                    tiposDeportesSet.Add(tipo);
                }
                tiposDeporte.Add(tipo);
            }
            dbContext.SaveChanges();

            // Insertar Competiciones si no existen
            var competicionesSet = dbContext.Set<Competicion>();
            if (competicionesSet.FirstOrDefault(c => c.Nombre == "Torneo Otoño Pádel 2026") == null) {
                var competicion1 = new Competicion {
                    Nombre = "Torneo Otoño Pádel 2026",
                    Lugar = "Pabellón Universitario IMD",
                    Fecha = DateTime.Now.AddDays(15),
                    Plazas = 16,
                    Precio = 15.50m,
                    TipoDeporteId = tiposDeporte[0].Id
                };
                competicionesSet.Add(competicion1);
            }

            if (competicionesSet.FirstOrDefault(c => c.Nombre == "Liga Local Tenis Individual") == null) {
                var competicion2 = new Competicion {
                    Nombre = "Liga Local Tenis Individual",
                    Lugar = "Club Tenis Albacete",
                    Fecha = DateTime.Now.AddDays(30),
                    Plazas = 32,
                    Precio = 20.00m,
                    TipoDeporteId = tiposDeporte[1].Id
                };
                competicionesSet.Add(competicion2);
            }

            // Insertar Clases Deportivas si no existen
            var clasesSet = dbContext.Set<ClaseDeportiva>();
            if (clasesSet.FirstOrDefault(c => c.Descripcion == "Clase introductoria de pádel para principiantes.") == null) {
                clasesSet.Add(new ClaseDeportiva("Clase introductoria de pádel para principiantes.", DateTime.Now.AddDays(3).Date.AddHours(10), "Carlos Ruiz", "Principiante", 12, 8.00m, tiposDeporte[0].Id, "Pista Cubierta 1 — IMD"));
            }

            if (clasesSet.FirstOrDefault(c => c.Descripcion == "Clase de tenis nivel intermedio, enfocada en el saque.") == null) {
                clasesSet.Add(new ClaseDeportiva("Clase de tenis nivel intermedio, enfocada en el saque.", DateTime.Now.AddDays(7).Date.AddHours(9), "Laura Gómez", "Intermedio", 10, 10.50m, tiposDeporte[1].Id, "Pista Exterior Tenis — Campus"));
            }

            //it saves the modification of dbcontext to the database
            dbContext.SaveChanges();

            //Since EFCORE7, you can perform bulk updates with linq. Example:
            //dbContext.Set<ClaseDeportiva>().ExecuteUpdate(s => s.SetProperty(c => c.PlazasDisponibles, c => c.PlazasDisponibles + 5));
        }

        public static void SeedInscripciones(ApplicationDbContext dbContext, ApplicationUser user) 
        {
            if (user == null) return;

            // Inscribirse a Competición
            if (!dbContext.Inscripciones.Any(i => i.CompeticionesInscritas.Any())) {
                var competicion = dbContext.Set<Competicion>().FirstOrDefault();
                if (competicion != null) {
                    var inscripcionComp = new Inscripcion(user.Name, user.Surname, user.DNI ?? "12345678Z", "600112233", DateTime.Now, MetodoPago.Tarjeta, competicion.Precio, "**** **** **** 4321");
                    inscripcionComp.CompeticionesInscritas = new List<CompeticionInscrita>();
                    
                    var compInscrita = new CompeticionInscrita {
                        Competicion = competicion,
                        Inscripcion = inscripcionComp,
                        ProblemasFisicos = "Ninguno"
                    };
                    inscripcionComp.CompeticionesInscritas.Add(compInscrita);
                    dbContext.Inscripciones.Add(inscripcionComp);
                }
            }

            // Inscribirse a Clase Deportiva
            if (!dbContext.Inscripciones.Any(i => i.ClasesInscritas.Any())) {
                var clase = dbContext.Set<ClaseDeportiva>().FirstOrDefault();
                if (clase != null) {
                    var inscripcionClase = new Inscripcion(user.Name, user.Surname, user.DNI ?? "12345678Z", "600112233", DateTime.Now, MetodoPago.Bizum, clase.PrecioUnitario * 1, "Bizum confirmado — ref. 20261002");
                    inscripcionClase.ClasesInscritas = new List<ClaseInscrita>();

                    var claseInscrita = new ClaseInscrita(clase.Id, 0, 1, clase.PrecioUnitario, "Sin observaciones.") {
                        ClaseDeportiva = clase,
                        Inscripcion = inscripcionClase
                    };
                    inscripcionClase.ClasesInscritas.Add(claseInscrita);
                    
                    // Update stock/places
                    clase.PlazasDisponibles -= claseInscrita.PlazasReservadas;
                    
                    dbContext.Inscripciones.Add(inscripcionClase);
                }
            }

            dbContext.SaveChanges();
        }
    }
}