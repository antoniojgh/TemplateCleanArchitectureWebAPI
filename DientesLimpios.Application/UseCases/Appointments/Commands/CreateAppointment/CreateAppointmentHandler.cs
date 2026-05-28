using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentHandler(IApplicationDbContext db, IAppointmentRepository repository, ILogger<CreateAppointmentHandler> logger) : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
            "Creating appointment for Patient {PatientId} with Dentist {DentistId}",
            request.PatientId, request.DentistId);

            var appointmentOverlaps = await repository.AppointmentOverlaps(request.DentistId, request.StartDate, request.EndDate, cancellationToken);

            if (appointmentOverlaps)
                return Result.Failure<Guid>(DomainErrors.Appointment.Overlapping);

            // Construct Appointment through its factory.
            var appointmentResult = Appointment.Create(request.PatientId, request.DentistId, request.OfficeId, request.StartDate, request.EndDate, DateTime.UtcNow);

            if (appointmentResult.IsFailure)
                return Result.Failure<Guid>(appointmentResult.Error);

            var appointment = appointmentResult.Value;

            db.Appointments.Add(appointment);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Appointment created successfully with ID: {AppointmentId}", appointment.Id);

            return Result.Success(appointment.Id);
        }

    }
}
