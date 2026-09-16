using DientesLimpios.Application.Configuration;
using DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders;
using DientesLimpios.Application.Utilities.Mediator;
using Microsoft.Extensions.Options;

namespace DientesLimpios.API.Jobs
{
    public class AppointmentReminderJob(IServiceScopeFactory scopeFactory, IOptions<ClinicOptions> clinicOptions,
                TimeProvider timeProvider, ILogger<AppointmentReminderJob> logger) : BackgroundService
    {
        // The clinic's time zone, from configuration, so the job and the use case that reads
        // "tomorrow" can never disagree about where the clinic is.
        private readonly TimeZoneInfo _clinicTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(clinicOptions.Value.TimeZoneId);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AppointmentReminderJob started. Waiting for the 08:00 trigger at the clinic ({TimeZoneId}).",
                _clinicTimeZone.Id);

            // While cancellation has not been requested
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, _clinicTimeZone);

                // If it is 8 AM at the clinic
                if (now.Hour == 8)
                {
                    logger.LogInformation("Triggering daily appointment reminders at {Time}", now);

                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await mediator.Send(new SendAppointmentRemindersCommand(), stoppingToken);

                        logger.LogInformation("Daily reminders command dispatched successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to execute daily reminders job.");
                    }
                }

                // Wait one hour before checking again
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

    }
}
