using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Events;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Appointments.Events
{
    public sealed class AppointmentCreatedEmailHandler: IDomainEventHandler<AppointmentCreatedEvent>
    {
        private readonly IAppointmentRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AppointmentCreatedEmailHandler> _logger;

        public AppointmentCreatedEmailHandler(
            IAppointmentRepository repository,
            INotificationService notificationService,
            ILogger<AppointmentCreatedEmailHandler> logger)
        {
            _repository = repository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(AppointmentCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                var appointment = await _repository.GetById(domainEvent.AppointmentId, cancellationToken);

                if (appointment is null)
                {
                    _logger.LogWarning(
                        "Appointment {AppointmentId} not found while handling AppointmentCreatedEvent.",
                        domainEvent.AppointmentId);
                    return;
                }

                var dto = appointment.ADto();

                await _notificationService.SendAppointmentConfirmation(dto);

                _logger.LogInformation(
                    "Confirmation email sent to {Email} for appointment {AppointmentId}.",
                    dto.PatientEmail, dto.Id);
            }
            catch (Exception ex)
            {
                // Best-effort. The appointment is already saved.
                _logger.LogError(ex,
                    "Failed to send confirmation email for appointment {AppointmentId}.",
                    domainEvent.AppointmentId);
            }
        }
    }

}
