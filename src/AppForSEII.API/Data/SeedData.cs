using System;
using System.Collections.Generic;
using System.Linq;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Identity;
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

            // --- INICIALIZACIÓN DE DATOS PARA COMPETICIONES E INSCRIPCIONES ---
            try {
                SeedTiposDeporteYCompeticiones(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding TiposDeporte and Competiciones in the Database.");
            }

            try {
                var user = dbContext.Users.OfType<ApplicationUser>().FirstOrDefault(u => u.UserName == "peter@uclm.es");
                SeedInscripcion(dbContext, user);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding an Inscripcion in the Database.");
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager, List<string> roles) 
        {
            foreach (string roleName in roles) {
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
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "Password1234%");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }

            if (userManager.FindByNameAsync("peter@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "OtherPass12$");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[2]).Wait();
                }
            }
        }

        // =========================================================================
        // MÉTODO: SeedTiposDeporteYCompeticiones
        // =========================================================================
        public static void SeedTiposDeporteYCompeticiones(ApplicationDbContext dbContext) 
        {
            string[] nombresDeportes = ["Pádel", "Tenis", "Baloncesto", "Fútbol 7"];
            List<TipoDeporte> tiposDeporte = new List<TipoDeporte>();

            foreach (string nombreDeporte in nombresDeportes) {
                var tipo = dbContext.TiposDeportes.FirstOrDefault(t => t.Nombre == nombreDeporte);
                if (tipo == null) {
                    tipo = new TipoDeporte { Nombre = nombreDeporte };
                    dbContext.TiposDeportes.Add(tipo);
                }
                tiposDeporte.Add(tipo);
            }
            dbContext.SaveChanges();

            // Insertar Competiciones si no existen
            if (dbContext.Competiciones.FirstOrDefault(c => c.Nombre == "Torneo Otoño Pádel 2026") == null) {
                var competicion1 = new Competicion {
                    Nombre = "Torneo Otoño Pádel 2026",
                    Lugar = "Pabellón Universitario IMD",
                    Fecha = DateTime.Now.AddDays(15),
                    Plazas = 16,
                    Precio = 15.50m,
                    TipoDeporteId = tiposDeporte[0].Id
                };
                dbContext.Competiciones.Add(competicion1);
            }

            if (dbContext.Competiciones.FirstOrDefault(c => c.Nombre == "Liga Local Tenis Individual") == null) {
                var competicion2 = new Competicion {
                    Nombre = "Liga Local Tenis Individual",
                    Lugar = "Club Tenis Albacete",
                    Fecha = DateTime.Now.AddDays(30),
                    Plazas = 32,
                    Precio = 20.00m,
                    TipoDeporteId = tiposDeporte[1].Id
                };
                dbContext.Competiciones.Add(competicion2);
            }

            dbContext.SaveChanges();
        }

        // =========================================================================
        // MÉTODO: SeedInscripcion
        // =========================================================================
        public static void SeedInscripcion(ApplicationDbContext dbContext, ApplicationUser user) 
        {
            // Verificamos si existe al menos una inscripción creada
            if (!dbContext.Inscripciones.Any()) {
                var competicion = dbContext.Competiciones.FirstOrDefault();

                if (competicion != null) {
                    var inscripcion = new Inscripcion {
                        NombreUsuario = user != null ? user.Nombre : "Peter",
                        ApellidosUsuario = user != null ? user.Apellidos : "Jackson",
                        DNI = "12345678Z",
                        Telefono = "600112233",
                        FechaInscripcion = DateTime.Now,
                        MetodoPago = "Tarjeta de Crédito",
                        PrecioTotal = competicion.Precio,
                        DatosPago = "**** **** **** 4321",
                        CompeticionesInscritas = new List<CompeticionInscrita>()
                    };

                    var competicionInscrita = new CompeticionInscrita {
                        Competicion = competicion,
                        Inscripcion = inscripcion,
                        ProblemasFisicos = "Ninguno"
                    };

                    inscripcion.CompeticionesInscritas.Add(competicionInscrita);
                    dbContext.Inscripciones.Add(inscripcion);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}