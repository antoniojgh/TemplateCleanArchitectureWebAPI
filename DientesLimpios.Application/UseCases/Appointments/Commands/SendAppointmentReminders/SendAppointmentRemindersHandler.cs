using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders
{
    public class SendAppointmentRemindersHandler(IAppointmentRepository repository,
                INotificationService notificationService, ILogger<SendAppointmentRemindersHandler> logger) : IRequestHandler<SendAppointmentRemindersCommand, Result>
    {
        public async Task<Result> Handle(SendAppointmentRemindersCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Sending appointment reminders for date: {Date}", DateTime.UtcNow.Date.AddDays(1));

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            var filter = new AppointmentFilterDTO
            {
                StartDate = tomorrow,
                AppointmentStatus = AppointmentStatus.Scheduled
            };

            var appointments = await repository.GetFiltered(filter, cancellationToken);

            foreach (var appointment in appointments)
            {
                var appointmentDTO = appointment.ADto();
                await notificationService.SendAppointmentReminder(appointmentDTO);
            }

            logger.LogInformation("Appointment reminders sent successfully for {AppointmentCount} appointments", appointments.Count());

            return Result.Success();
        }
    }
}
