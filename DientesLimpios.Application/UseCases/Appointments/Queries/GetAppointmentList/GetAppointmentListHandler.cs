using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentList
{
    public class GetAppointmentListHandler(IAppointmentRepository repository, ILogger<GetAppointmentListHandler> logger) : IRequestHandler<GetAppointmentListQuery, Result<List<AppointmentListDTO>>>
    {
        public async Task<Result<List<AppointmentListDTO>>> Handle(GetAppointmentListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving appointment list");

            var appointments = await repository.GetFiltered(request);

            var appointmentsDTO = appointments.Select(appointment => appointment.ADto()).ToList();

            logger.LogInformation("Appointment list retrieved successfully with {AppointmentCount} appointments", appointmentsDTO.Count);

            return Result.Success(appointmentsDTO);
        }
    }
}
