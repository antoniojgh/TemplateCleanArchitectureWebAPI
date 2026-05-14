using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentHandler(IAppointmentRepository repository, IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<CreateAppointmentHandler> logger) : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
            "Creating appointment for Patient {PatientId} with Dentist {DentistId}",
            request.PatientId, request.DentistId);

            var appointmentOverlaps = await repository.AppointmentOverlaps(request.DentistId, request.StartDate, request.EndDate);

            if (appointmentOverlaps)
                return Result.Failure<Guid>(DomainErrors.Appointment.Overlapping);

            // Construct Appointment through its factory.
            var appointmentResult = Appointment.Create(request.PatientId, request.DentistId, request.OfficeId, request.StartDate, request.EndDate, DateTime.UtcNow);

            if (appointmentResult.IsFailure)
                return Result.Failure<Guid>(appointmentResult.Error);

            var appointment = appointmentResult.Value;

            var createdAppointment = await repository.Add(appointment);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Appointment created successfully with ID: {AppointmentId}", createdAppointment.Id);

            // Email confirmation — best-effort, must not fail the appointment creation.
            try
            {
                var appointmentDb = await repository.GetById(createdAppointment.Id);
                var notificationDTO = appointmentDb!.ADto();

                await notificationService.SendAppointmentConfirmation(notificationDTO);

                logger.LogInformation(
                    "Confirmation email sent to {Email}", notificationDTO.PatientEmail);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Appointment {AppointmentId} created but confirmation email failed to send.",
                    createdAppointment.Id);
            }

            return Result.Success(createdAppointment.Id);
        }

    }
}
