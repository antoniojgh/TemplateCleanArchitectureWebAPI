using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail
{
    public class GetAppointmentDetailHandler(IAppointmentRepository repository, ILogger<GetAppointmentDetailHandler> logger) : IRequestHandler<GetAppointmentDetailQuery, Result<AppointmentDetailDTO>>
    {
        public async Task<Result<AppointmentDetailDTO>> Handle(GetAppointmentDetailQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo detalle de appointment con ID: {AppointmentId}", request.Id);

            var appointment = await repository.GetById(request.Id);

            if (appointment is null)
                return Result.Failure<AppointmentDetailDTO>(DomainErrors.Appointment.NotFound);

            logger.LogInformation("Detail de appointment obtenido correctamente con ID: {AppointmentId}", request.Id);

            return Result.Success(appointment.ADto());
        }
    }
}
