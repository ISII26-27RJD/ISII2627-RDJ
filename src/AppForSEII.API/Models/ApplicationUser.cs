using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AppForSEII.API.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
        }

        public ApplicationUser(string id, string name, string surname, string userName, string dni, int age, string sex)
        {
            Id = id;
            Name = name;
            Surname = surname;
            UserName = userName;
            Email = userName;
            DNI = dni;
            Age = age;
            Sex = sex;
        }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Apellidos")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^[0-9]{8}[A-Za-z]$", ErrorMessage = "El DNI debe tener un formato válido (8 números y 1 letra).")]
        [System.ComponentModel.DataAnnotations.Display(Name = "DNI")]
        public string DNI { get; set; }

        // La edad debe ser un valor positivo y razonable
        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(1, 120, ErrorMessage = "La edad debe estar entre 1 y 120 años.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Edad")]
        public int Age { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio.")]
        [StringLength(20, ErrorMessage = "El sexo no puede superar los 20 caracteres.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Sexo")]
        public string Sex { get; set; }

        // Email, PhoneNumber y UserName son heredados de IdentityUser

        // --- Propiedad de navegación según la relación del diagrama ---
        public IList<Inscripcion> Inscripciones { get; set; }
    }
}
