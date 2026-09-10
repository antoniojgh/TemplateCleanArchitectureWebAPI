using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentHandler(IAppointmentRepository repository, ILogger<CreateAppointmentHandler> logger) : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
            "Creating appointment for Patient {PatientId} with Dentist {DentistId}",
            request.PatientId, request.DentistId);

            // The overlap check, the construction of the aggregate and the insert all happen
            // inside one transaction, serialised per dentist.
            var addResult = await repository.AddIfNoOverlap(request.PatientId, request.DentistId, request.OfficeId, request.StartDate, request.EndDate, cancellationToken);

            if (addResult.IsFailure)
                return Result.Failure<Guid>(addResult.Error);

            logger.LogInformation("Appointment created successfully with ID: {AppointmentId}", addResult.Value);

            return Result.Success(addResult.Value);
        }
    }
}
