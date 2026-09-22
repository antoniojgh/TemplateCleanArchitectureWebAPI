using DientesLimpios.Application.Configuration;
using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Appointments.DTOs;
using DientesLimpios.Application.UseCases.Appointments.Utilities;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders
{
    public class SendAppointmentRemindersHandler(IApplicationDbContext db,
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

            var reminders = await (
                from a in db.Appointments.ApplyFilter(filter).OrderBy(a => a.TimeInterval.Start)
                join p in db.Patients on a.PatientId equals p.Id
                join d in db.Dentists on a.DentistId equals d.Id
                join o in db.Offices on a.OfficeId equals o.Id
                select new AppointmentReminderDTO
                {
                    Id = a.Id,
                    Date = a.TimeInterval.Start,
                    Patient = p.Name,
                    PatientEmail = p.Email.Value,
                    Dentist = d.Name,
                    Office = o.Name
                }).ToListAsync(cancellationToken);

            foreach (var reminder in reminders)
                await notificationService.SendAppointmentReminder(reminder, cancellationToken);

            logger.LogInformation("Appointment reminders sent successfully for {AppointmentCount} appointments", reminders.Count);
            return Result.Success();
        }
    }
}
