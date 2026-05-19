using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CompleteAppointment
{
    public class CompleteAppointmentHandler(IApplicationDbContext db, IAppointmentRepository repository, ILogger<CompleteAppointmentHandler> logger) : IRequestHandler<CompleteAppointmentCommand, Result>
    {
        public async Task<Result> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Completing appointment with ID: {AppointmentId}", request.Id);

            var appointment = await repository.GetById(request.Id, cancellationToken);

            if (appointment is null)
                return Result.Failure(DomainErrors.Appointment.NotFound);

            var completarResult = appointment.Complete();
            if (completarResult.IsFailure)
                return completarResult;

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Appointment completed successfully with ID: {AppointmentId}", request.Id);

            return Result.Success();
        }
    }
}
