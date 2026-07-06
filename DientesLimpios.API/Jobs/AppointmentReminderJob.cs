using DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders;
using DientesLimpios.Application.Utilities.Mediator;

namespace DientesLimpios.API.Jobs
{
    public class AppointmentReminderJob(IServiceScopeFactory scopeFactory, ILogger<AppointmentReminderJob> logger) : BackgroundService
    {
        // Spain timezone (Central European Time / CEST)
        private readonly TimeZoneInfo _spainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AppointmentReminderJob started. Waiting for 8:00 AM EST trigger.");

            // While cancellation has not been requested
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _spainTimeZone);

                // If it is 8 AM in Spain
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
