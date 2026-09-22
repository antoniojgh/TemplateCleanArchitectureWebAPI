using System.ComponentModel.DataAnnotations;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.API.DTOs.Offices
{
    public class CreateOfficeDTO
    {
        [Required]
        [StringLength(Office.NameMaxLength)]
        public required string Name { get; set; }
    }
}
