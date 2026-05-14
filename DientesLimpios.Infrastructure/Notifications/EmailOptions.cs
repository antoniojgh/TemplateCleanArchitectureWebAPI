using System.ComponentModel.DataAnnotations;

namespace DientesLimpios.Infrastructure.Notifications
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        [Required]
        public string Host { get; init; } = "";

        [Range(1, 65535)]
        public int Port { get; init; }

        [Required, EmailAddress]
        public string Email { get; init; } = "";

        [Required]
        public string Password { get; init; } = "";
    }
}
