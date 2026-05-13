using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentHandler(IAppointmentRepository repository, IUnitOfWork unitOfWork, ILogger<CancelAppointmentHandler> logger) : IRequestHandler<CancelAppointmentCommand, Result>
    {

        public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Cancelando appointment con ID: {AppointmentId}", request.Id);

            var appointment = await repository.GetById(request.Id);

            if (appointment is null)
                return Result.Failure(DomainErrors.Appointment.NotFound);

            var cancelarResult = appointment.Cancel();
            if (cancelarResult.IsFailure)
                return cancelarResult;

            await repository.Update(appointment);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Appointment cancelada correctamente con ID: {AppointmentId}", request.Id);

            return Result.Success();

        }
    }
}
