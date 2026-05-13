using System.ComponentModel.DataAnnotations;

namespace DientesLimpios.API.DTOs.Offices
{
    public class CreateOfficeDTO
    {
        [Required]
        [StringLength(150)]
        public required string Name { get; set; }
    }
}
