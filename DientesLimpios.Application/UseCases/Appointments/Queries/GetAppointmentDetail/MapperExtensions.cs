using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail
{
    public static class MapperExtensions
    {
        public static AppointmentDetailDTO ADto(this Appointment appointment)
        {
            var dto = new AppointmentDetailDTO
            {
                Id = appointment.Id,
                StartDate = appointment.TimeInterval.Start,
                EndDate = appointment.TimeInterval.End,
                Office = appointment.Office!.Name,
                Dentist = appointment.Dentist!.Name,
                Patient = appointment.Patient!.Name,
                AppointmentStatus = appointment.Status.ToString()
            };
            return dto;
        }

    }
}
