using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders
{
    public static class MapperExtensions
    {
        public static AppointmentReminderDTO ADto(this Appointment appointment)
        {
            return new AppointmentReminderDTO
            {
                Id = appointment.Id,
                Date = appointment.TimeInterval.Start,
                Patient = appointment.Patient!.Name,
                PatientEmail = appointment.Patient.Email.Value,
                Office = appointment.Office!.Name,
                Dentist = appointment.Dentist!.Name
            };
        }

    }
}
