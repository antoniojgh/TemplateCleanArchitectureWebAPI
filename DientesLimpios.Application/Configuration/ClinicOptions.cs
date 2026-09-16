using System.ComponentModel.DataAnnotations;

namespace DientesLimpios.Application.Configuration
{
    // Where the clinic is. "Tomorrow" in a reminder means a day at the clinic, not a UTC
    // day, so the rule needs a time zone - and that is a business setting, not a server one.
    public class ClinicOptions
    {
        public const string SectionName = "Clinic";

        [Required]
        public string TimeZoneId { get; init; } = "";
    }
}
