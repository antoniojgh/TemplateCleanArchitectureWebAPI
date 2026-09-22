using System.ComponentModel.DataAnnotations;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.API.DTOs.Dentists
{
    public class CreateDentistDTO
    {
        [Required]
        [StringLength(Dentist.NameMaxLength)]
        public required string Name { get; set; }

        [Required]
        [StringLength(DientesLimpios.Domain.ValueObjects.Email.MaxLength)]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
