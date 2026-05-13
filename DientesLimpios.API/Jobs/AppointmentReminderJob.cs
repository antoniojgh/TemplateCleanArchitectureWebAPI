using DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders;
using DientesLimpios.Application.Utilities.Mediator;

namespace DientesLimpios.API.Jobs
{
    public class AppointmentReminderJob(IServiceScopeFactory scopeFactory, ILogger<AppointmentReminderJob> logger) : BackgroundService
    {
        // Zona horaria de República Dominicana (Eastern Standard Time)
        private readonly TimeZoneInfo zonaHorariaRD = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AppointmentReminderJob started. Waiting for 8:00 AM EST trigger.");

            // Mientras no se solicite la cancelación del token
            while (!stoppingToken.IsCancellationRequested)
            {
                var ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaRD);

                // Si son las 8 AM en República Dominicana
                if (ahora.Hour == 8)
                {
                    logger.LogInformation("Triggering daily appointment reminders at {Time}", ahora);

                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await mediator.Send(new SendAppointmentRemindersCommand());

                        logger.LogInformation("Daily reminders command dispatched successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to execute daily reminders job.");
                    }
                }

                // Esperar una hora antes de volver a verificar
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

    }
}
