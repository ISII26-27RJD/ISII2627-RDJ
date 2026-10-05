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
        public static async Task InitializeAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger logger) 
        {
            List<string> rolesNames = new List<string> { "Administrator", "Employee", "Customer" };

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            try {
                await SeedRolesAsync(roleManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the roles in the Database.");
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            try {
                await SeedUsersAsync(userManager, rolesNames);
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
                var user = await dbContext.Users.OfType<ApplicationUser>()
                    .FirstOrDefaultAsync(u => u.UserName == "peter@uclm.es");

                //it initializes the database with an Inscripcion
                SeedInscripciones(dbContext, user);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding an Inscripcion in the Database.");
            }

            try {
                // Vamos a iniciar la base de datos con instancias de Pista y Reserva, para la comprobación de funcionalidades.
                // Por ahora dejamos la llamada al método comentada.
                //SeedPistasYReservas(dbContext); //DESCOMENTAR A UN FUTURO
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding Pistas and Reservas in the Database.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, IEnumerable<string> roles)
        {
            foreach (string roleName in roles) {
                //it checks such role does not exist in the database 
                if (!await roleManager.RoleExistsAsync(roleName)) {
                    IdentityRole role = new IdentityRole {
                        Name = roleName,
                        NormalizedName = roleManager.NormalizeKey(roleName)
                    };
                    IdentityResult roleResult = await roleManager.CreateAsync(role);
                    EnsureIdentitySuccess(roleResult, $"creating role '{roleName}'");
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, IReadOnlyList<string> roles)
        {
            await SeedUserAsync(userManager, new ApplicationUser("1", "Elena", "Navarro Martínez",
                "elena@uclm.es", "11111111A", 45, "Femenino"), "Password1234%", roles[0]);
            await SeedUserAsync(userManager, new ApplicationUser("2", "Gregorio", "Diaz Descalzo",
                "gregorio@uclm.es", "22222222B", 50, "Masculino"), "APassword1234%", roles[1]);
            await SeedUserAsync(userManager, new ApplicationUser("3", "Peter", "Jackson",
                "peter@uclm.es", "33333333C", 30, "Masculino"), "OtherPass12$", roles[2]);
        }

        private static async Task SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser newUser,
            string password,
            string role)
        {
            var user = await userManager.FindByNameAsync(newUser.UserName);
            if (user == null) {
                newUser.EmailConfirmed = true;
                var result = await userManager.CreateAsync(newUser, password);
                EnsureIdentitySuccess(result, $"creating user '{newUser.UserName}'");
                user = newUser;
            }

            if (!await userManager.IsInRoleAsync(user, role)) {
                var result = await userManager.AddToRoleAsync(user, role);
                EnsureIdentitySuccess(result, $"assigning role '{role}' to user '{user.UserName}'");
            }
        }

        private static void EnsureIdentitySuccess(IdentityResult result, string operation)
        {
            if (!result.Succeeded) {
                var errors = string.Join("; ", result.Errors.Select(error =>
                    $"{error.Code}: {error.Description}"));
                throw new InvalidOperationException($"Identity error while {operation}: {errors}");
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

        public static void SeedInscripciones(ApplicationDbContext dbContext, ApplicationUser? user) 
        {
            if (user == null) return;

            // Inscribirse a Competición
            if (!dbContext.Inscripciones.Any(i =>
                i.DNI == user.DNI &&
                i.CompeticionesInscritas.Any())) {
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
            if (!dbContext.Inscripciones.Any(i =>
                i.DNI == user.DNI &&
                i.ClasesInscritas.Any())) {
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

        public static void SeedPistasYReservas(ApplicationDbContext dbContext) //Caso de Uso Reservar pista
        {
            //Voy a reutilizar deportes que ya existen en la base de datos
            var tipoPadel = dbContext.Set<TipoDeporte>().FirstOrDefault(t => t.Nombre == "Pádel");
            var tipoTenis = dbContext.Set<TipoDeporte>().FirstOrDefault(t => t.Nombre == "Tenis");
        }
    }
}