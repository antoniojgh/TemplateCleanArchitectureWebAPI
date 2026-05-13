using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public static class MapperExtensions
    {
        public static AppointmentConfirmationDTO ADto(this Appointment appointment)
        {
            return new AppointmentConfirmationDTO
            {
                Id = appointment.Id,
                Fecha = appointment.TimeInterval.Start,
                Patient = appointment.Patient!.Name,
                Patient_Email = appointment.Patient.Email.Value,
                Office = appointment.Office!.Name,
                Dentist = appointment.Dentist!.Name
            };
        }

    }
}
