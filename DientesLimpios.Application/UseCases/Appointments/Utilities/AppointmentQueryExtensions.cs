using DientesLimpios.Application.UseCases.Appointments.DTOs;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Enums;

namespace DientesLimpios.Application.UseCases.Appointments.Utilities
{
    // Shared by the appointment list query and the reminder command: both need the same
    // filter, and both compose a projection on top of it. It returns IQueryable on purpose —
    // nothing is executed here, so the caller decides what to select and when to materialize.
    internal static class AppointmentQueryExtensions
    {
        public static IQueryable<Appointment> ApplyFilter(
            this IQueryable<Appointment> query, AppointmentFilterDTO filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            if (filter.OfficeId is Guid officeId)
                query = query.Where(a => a.OfficeId == officeId);

            if (filter.DentistId is Guid dentistId)
                query = query.Where(a => a.DentistId == dentistId);

            if (filter.PatientId is Guid patientId)
                query = query.Where(a => a.PatientId == patientId);

            if (filter.AppointmentStatus is AppointmentStatus status)
                query = query.Where(a => a.Status == status);

            // Half-open window [Start, End): the same comparison operators the repository used.
            if (filter.StartDate is DateTime startDate)
                query = query.Where(a => a.TimeInterval.Start >= startDate);

            if (filter.EndDate is DateTime endDate)
                query = query.Where(a => a.TimeInterval.End < endDate);

            return query;
        }
    }
}
