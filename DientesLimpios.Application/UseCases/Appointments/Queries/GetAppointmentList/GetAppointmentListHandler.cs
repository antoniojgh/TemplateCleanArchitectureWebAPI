using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.Extensions.Logging;
using DientesLimpios.Application.UseCases.Appointments.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentList
{
    public class GetAppointmentListHandler(IApplicationDbContext db, ILogger<GetAppointmentListHandler> logger)
        : IRequestHandler<GetAppointmentListQuery, Result<List<AppointmentListDTO>>>
    {
        public async Task<Result<List<AppointmentListDTO>>> Handle(
            GetAppointmentListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving appointment list");

            var appointments = await (
                from a in db.Appointments.ApplyFilter(request).OrderBy(a => a.TimeInterval.Start)
                join p in db.Patients on a.PatientId equals p.Id
                join d in db.Dentists on a.DentistId equals d.Id
                join o in db.Offices on a.OfficeId equals o.Id
                select new AppointmentListDTO
                {
                    Id = a.Id,
                    Patient = p.Name,
                    Dentist = d.Name,
                    Office = o.Name,
                    StartDate = a.TimeInterval.Start,
                    EndDate = a.TimeInterval.End,
                    AppointmentStatus = a.Status.ToString()
                }).ToListAsync(cancellationToken);

            logger.LogInformation("Appointment list retrieved successfully with {AppointmentCount} appointments",
                appointments.Count);

            return Result.Success(appointments);
        }
    }
}
