using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Commands.RescheduleAppointment
{
    public class RescheduleAppointmentHandler(IAppointmentRepository repository, ILogger<RescheduleAppointmentHandler> logger) : IRequestHandler<RescheduleAppointmentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Rescheduling appointment with ID: {AppointmentId}", request.Id);

            var appointment = await repository.GetById(request.Id, cancellationToken);

            if (appointment is null)
                return Result.Failure<Guid>(DomainErrors.Appointment.NotFound);

            // The overlap check, the reschedule of the aggregate happen
            // inside one transaction, serialised per dentist.
            var addResult = await repository.RescheduleIfNoOverlap(appointment, request.StartDate, request.EndDate, cancellationToken);

            if (addResult.IsFailure)
                return Result.Failure<Guid>(addResult.Error);

            logger.LogInformation("Appointment rescheduled successfully with ID: {AppointmentId}", addResult.Value);

            return Result.Success(addResult.Value);

        }
    }
}
