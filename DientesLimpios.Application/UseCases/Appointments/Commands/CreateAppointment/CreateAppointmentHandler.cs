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
            "Creando appointment para Patient {PatientId} con Dentist {DentistId}",
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

            logger.LogInformation("Appointment creada correctamente con ID: {AppointmentId}", createdAppointment.Id);

            // Email confirmation — best-effort, must not fail the appointment creation.
            try
            {
                var appointmentDb = await repository.GetById(createdAppointment.Id);
                var notificationDTO = appointmentDb!.ADto();

                await notificationService.SendAppointmentConfirmation(notificationDTO);

                logger.LogInformation(
                    "Email de confirmación enviado a {Email}", notificationDTO.Patient_Email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Appointment {AppointmentId} creada pero falló el envío del email de confirmación.",
                    createdAppointment.Id);
            }

            return Result.Success(createdAppointment.Id);
        }

    }
}
