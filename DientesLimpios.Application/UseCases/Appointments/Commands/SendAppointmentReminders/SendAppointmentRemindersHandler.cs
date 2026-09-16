using DientesLimpios.Application.Configuration;
using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders
{
    public class SendAppointmentRemindersHandler(IAppointmentRepository repository,
                INotificationService notificationService, TimeProvider timeProvider,
                IOptions<ClinicOptions> clinicOptions,
                ILogger<SendAppointmentRemindersHandler> logger) : IRequestHandler<SendAppointmentRemindersCommand, Result>
    {
        public async Task<Result> Handle(SendAppointmentRemindersCommand request, CancellationToken cancellationToken)
        {
            // "Tomorrow" is a day at the clinic, not a UTC day. Taking the boundary in UTC
            // skipped the appointments between local midnight and 02:00 in summer, and
            // reminded the ones just after midnight the following day a day early.
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(clinicOptions.Value.TimeZoneId);

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, timeZone);
            var localTomorrow = localNow.Date.AddDays(1);

            logger.LogInformation("Sending appointment reminders for {LocalDate} at the clinic ({TimeZoneId})",
                localTomorrow, timeZone.Id);

            // Appointments are stored in UTC, so the window has to be converted back.
            var filter = new AppointmentFilterDTO
            {
                StartDate = TimeZoneInfo.ConvertTimeToUtc(localTomorrow, timeZone),
                EndDate = TimeZoneInfo.ConvertTimeToUtc(localTomorrow.AddDays(1), timeZone),
                AppointmentStatus = AppointmentStatus.Scheduled
            };

            var appointments = await repository.GetFiltered(filter, cancellationToken);

            foreach (var appointment in appointments)
            {
                var appointmentDTO = appointment.ADto();
                await notificationService.SendAppointmentReminder(appointmentDTO, cancellationToken);
            }

            logger.LogInformation("Appointment reminders sent successfully for {AppointmentCount} appointments", appointments.Count());

            return Result.Success();
        }
    }
}
