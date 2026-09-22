using System.ComponentModel.DataAnnotations;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.API.DTOs.Offices
{
    public class UpdateOfficeDTO
    {
        [Required]
        [StringLength(Office.NameMaxLength)]
        public required string Name { get; set; }
    }
}
