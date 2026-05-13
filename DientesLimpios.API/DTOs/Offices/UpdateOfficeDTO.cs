using System.ComponentModel.DataAnnotations;

namespace DientesLimpios.API.DTOs.Offices
{
    public class UpdateOfficeDTO
    {
        [Required]
        [StringLength(150)]
        public required string Name { get; set; }
    }
}
