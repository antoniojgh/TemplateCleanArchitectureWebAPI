using System.ComponentModel.DataAnnotations;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.API.DTOs.Patients
{
    public class CreatePatientDTO
    {
        [Required]
        [StringLength(Patient.NameMaxLength)]
        public required string Name { get; set; }

        [Required]
        [StringLength(DientesLimpios.Domain.ValueObjects.Email.MaxLength)]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
