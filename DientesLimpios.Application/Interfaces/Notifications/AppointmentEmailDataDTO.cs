using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Application.Interfaces.Notifications
{
    public class AppointmentEmailDataDTO
    {
        public required Guid Id { get; set; }
        public required string Patient { get; set; }
        public required string Patient_Email { get; set; }
        public required string Dentist { get; set; }
        public required string Office { get; set; }
        public required DateTime Fecha { get; set; }
    }
}
