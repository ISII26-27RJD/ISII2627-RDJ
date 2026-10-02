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
                SeedTiposDeporte(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding TiposDeporte in the Database.");
            }

            try {
                SeedCompeticiones(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding Competiciones in the Database.");
            }

            try {
                var user = dbContext.Users.OfType<ApplicationUser>().FirstOrDefault(u => u.UserName == "peter@uclm.es");
                SeedInscripcion(dbContext, user);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding an Inscripcion in the Database.");
            }

            // --- INICIALIZACIÓN DE DATOS PARA CLASES DEPORTIVAS ---
            try {
                SeedClasesDeportivas(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding ClasesDeportivas in the Database.");
            }

            try {
                var user = dbContext.Users.OfType<ApplicationUser>().FirstOrDefault(u => u.UserName == "peter@uclm.es");
                SeedInscripcionClaseDeportiva(dbContext, user);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding InscripcionClaseDeportiva in the Database.");
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
        public static void SeedTiposDeporte(ApplicationDbContext dbContext) 
        {
            string[] nombresDeportes = ["Pádel", "Tenis", "Baloncesto", "Fútbol 7"];

            foreach (string nombreDeporte in nombresDeportes) {
                if (dbContext.TiposDeportes.FirstOrDefault(t => t.Nombre == nombreDeporte) == null) {
                    dbContext.TiposDeportes.Add(new TipoDeporte { Nombre = nombreDeporte });
                }
            }
            dbContext.SaveChanges();
        }

        public static void SeedCompeticiones(ApplicationDbContext dbContext) 
        {
            var tipoPadel = dbContext.TiposDeportes.FirstOrDefault(t => t.Nombre == "Pádel");
            var tipoTenis = dbContext.TiposDeportes.FirstOrDefault(t => t.Nombre == "Tenis");

            if (tipoPadel != null && dbContext.Competiciones.FirstOrDefault(c => c.Nombre == "Torneo Otoño Pádel 2026") == null) {
                dbContext.Competiciones.Add(new Competicion {
                    Nombre = "Torneo Otoño Pádel 2026",
                    Lugar = "Pabellón Universitario IMD",
                    Fecha = DateTime.Now.AddDays(15),
                    Plazas = 16,
                    Precio = 15.50m,
                    TipoDeporteId = tipoPadel.Id
                });
            }

            if (tipoTenis != null && dbContext.Competiciones.FirstOrDefault(c => c.Nombre == "Liga Local Tenis Individual") == null) {
                dbContext.Competiciones.Add(new Competicion {
                    Nombre = "Liga Local Tenis Individual",
                    Lugar = "Club Tenis Albacete",
                    Fecha = DateTime.Now.AddDays(30),
                    Plazas = 32,
                    Precio = 20.00m,
                    TipoDeporteId = tipoTenis.Id
                });
            }

            dbContext.SaveChanges();
        }

        // =========================================================================
        // MÉTODO: SeedInscripcion (caso de uso: Inscribirse en Competición)
        // =========================================================================
        public static void SeedInscripcion(ApplicationDbContext dbContext, ApplicationUser user) 
        {
            // Verificamos si existe al menos una inscripción creada
            if (!dbContext.Inscripciones.Any()) {
                var competicion = dbContext.Set<Competicion>().FirstOrDefault();

                if (competicion != null) {
                    var inscripcion = new Inscripcion {
                        NombreUsuario = user != null ? user.Name : "Peter",
                        ApellidosUsuario = user != null ? user.Surname : "Jackson",
                        DNI = "12345678Z",
                        Telefono = "600112233",
                        FechaInscripcion = DateTime.Now,
                        MetodoPago = MetodoPago.Tarjeta,
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

<<<<<<< HEAD
        public static void SeedCompeticionesInscritas(ApplicationDbContext dbContext) 
        {
            if (!dbContext.CompeticionesInscritas.Any()) {
                var inscripcion = dbContext.Inscripciones.FirstOrDefault();
                var competicion = dbContext.Competiciones.FirstOrDefault();

                if (inscripcion != null && competicion != null) {
                    var competicionInscrita = new CompeticionInscrita {
                        InscripcionId = inscripcion.Id,
                        CompeticionId = competicion.Id,
                        ProblemasFisicos = "Ninguno"
                    };

                    dbContext.CompeticionesInscritas.Add(competicionInscrita);
                    dbContext.SaveChanges();
                }
            }
        }

=======
>>>>>>> 68a7bf1a3b393de76f29930dbb20f4c7232699a1
        // =========================================================================
        // MÉTODO: SeedClasesDeportivas (caso de uso: Apuntarse a Clase Deportiva)
        // =========================================================================
        public static void SeedClasesDeportivas(ApplicationDbContext dbContext)
        {
            // Solo insertamos si no hay clases deportivas aún
            if (dbContext.Set<ClaseDeportiva>().Any())
                return;

            // Recuperamos los TiposDeporte ya creados por SeedTiposDeporteYCompeticiones
            var tiposPadel      = dbContext.Set<TipoDeporte>().FirstOrDefault(t => t.Nombre == "Pádel");
            var tiposTenis      = dbContext.Set<TipoDeporte>().FirstOrDefault(t => t.Nombre == "Tenis");
            var tiposBaloncesto = dbContext.Set<TipoDeporte>().FirstOrDefault(t => t.Nombre == "Baloncesto");

            var clasesSet = dbContext.Set<ClaseDeportiva>();

            // Clase 1 — Pádel principiante
            if (tiposPadel != null) {
                clasesSet.Add(new ClaseDeportiva {
                    Descripcion     = "Clase introductoria de pádel para principiantes.",
                    FechaHora       = DateTime.Now.AddDays(3).Date.AddHours(10),
                    Lugar           = "Pista Cubierta 1 — IMD",
                    Monitor         = "Carlos Ruiz",
                    Nivel           = "Principiante",
                    PlazasDisponibles = 12,
                    PrecioUnitario  = 8.00m,
                    TipoDeporteId   = tiposPadel.Id
                });

                // Clase 2 — Pádel avanzado
                clasesSet.Add(new ClaseDeportiva {
                    Descripcion     = "Entrenamiento técnico avanzado de pádel.",
                    FechaHora       = DateTime.Now.AddDays(5).Date.AddHours(18),
                    Lugar           = "Pista Cubierta 2 — IMD",
                    Monitor         = "Carlos Ruiz",
                    Nivel           = "Avanzado",
                    PlazasDisponibles = 8,
                    PrecioUnitario  = 12.00m,
                    TipoDeporteId   = tiposPadel.Id
                });
            }

            // Clase 3 — Tenis intermedio
            if (tiposTenis != null) {
                clasesSet.Add(new ClaseDeportiva {
                    Descripcion     = "Clase de tenis nivel intermedio, enfocada en el saque.",
                    FechaHora       = DateTime.Now.AddDays(7).Date.AddHours(9),
                    Lugar           = "Pista Exterior Tenis — Campus",
                    Monitor         = "Laura Gómez",
                    Nivel           = "Intermedio",
                    PlazasDisponibles = 10,
                    PrecioUnitario  = 10.50m,
                    TipoDeporteId   = tiposTenis.Id
                });
            }

            // Clase 4 — Baloncesto principiante
            if (tiposBaloncesto != null) {
                clasesSet.Add(new ClaseDeportiva {
                    Descripcion     = "Iniciación al baloncesto: fundamentos y reglas básicas.",
                    FechaHora       = DateTime.Now.AddDays(10).Date.AddHours(17),
                    Lugar           = "Pabellón Principal — IMD",
                    Monitor         = "Marcos Díaz",
                    Nivel           = "Principiante",
                    PlazasDisponibles = 16,
                    PrecioUnitario  = 7.50m,
                    TipoDeporteId   = tiposBaloncesto.Id
                });
            }

            dbContext.SaveChanges();
        }

        // =========================================================================
        // MÉTODO: SeedInscripcionClaseDeportiva (caso de uso: Apuntarse a Clase Deportiva)
        // =========================================================================
        public static void SeedInscripcionClaseDeportiva(ApplicationDbContext dbContext, ApplicationUser user)
        {
            // Solo insertamos si no hay inscripciones con ClasesInscritas aún
            if (dbContext.Set<ClaseInscrita>().Any())
                return;

            var clase = dbContext.Set<ClaseDeportiva>().FirstOrDefault();
            if (clase == null || user == null)
                return;

            // Creamos la Inscripcion para el caso de uso de clase deportiva
            var inscripcion = new Inscripcion {
                NombreUsuario    = user.Name,
                ApellidosUsuario = user.Surname,
                DNI              = "12345678Z",
                Telefono         = "600112233",
                FechaInscripcion = DateTime.Now,
                MetodoPago       = MetodoPago.Bizum,
                PrecioTotal      = clase.PrecioUnitario * 1,   // 1 plaza reservada
                DatosPago        = "Bizum confirmado — ref. 20261002",
                ClasesInscritas  = new List<ClaseInscrita>()
            };

            var claseInscrita = new ClaseInscrita {
                ClaseDeportiva   = clase,
                Inscripcion      = inscripcion,
                Observaciones    = "Sin observaciones.",
                PlazasReservadas = 1,
                Precio           = clase.PrecioUnitario
            };

            inscripcion.ClasesInscritas.Add(claseInscrita);

            // Reducimos las plazas disponibles de la clase
            clase.PlazasDisponibles -= claseInscrita.PlazasReservadas;

            dbContext.Inscripciones.Add(inscripcion);
            dbContext.SaveChanges();
        }
    }
}