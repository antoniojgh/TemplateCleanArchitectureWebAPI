using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail
{
    public class GetAppointmentDetailHandler(IApplicationDbContext db, ILogger<GetAppointmentDetailHandler> logger) : IRequestHandler<GetAppointmentDetailQuery, Result<AppointmentDetailDTO>>
    {
        public async Task<Result<AppointmentDetailDTO>> Handle(GetAppointmentDetailQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving appointment detail with ID: {AppointmentId}", request.Id);

            var dto = await (from a in db.Appointments.AsNoTracking().Where(a => a.Id == request.Id)
                             join p in db.Patients on a.PatientId equals p.Id
                             join d in db.Dentists on a.DentistId equals d.Id
                             join o in db.Offices on a.OfficeId equals o.Id
                             select new AppointmentDetailDTO
                             {
                                 Id = a.Id,
                                 Patient = p.Name,
                                 Dentist = d.Name,
                                 Office = o.Name,
                                 StartDate = a.TimeInterval.Start,
                                 EndDate = a.TimeInterval.End,
                                 AppointmentStatus = a.Status.ToString()
                             })
                            .FirstOrDefaultAsync(cancellationToken);

            return dto is null ? Result.Failure<AppointmentDetailDTO>(DomainErrors.Appointment.NotFound)
                                 : Result.Success(dto);
        }
    }
}
