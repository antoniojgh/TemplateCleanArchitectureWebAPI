using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.EnviarRecordatorioCitas;
using MediatR;

namespace DientesLimpios.API.Jobs
{
    public class RecordatorioCitasJob(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        // Zona horaria de República Dominicana (Eastern Standard Time)
        private readonly TimeZoneInfo zonaHorariaRD = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Mientras no se solicite la cancelación del token
            while (!stoppingToken.IsCancellationRequested)
            {
                var ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaRD);

                // Si son las 8 AM en República Dominicana
                if (ahora.Hour == 8)
                {
                    using var scope = scopeFactory.CreateScope();
                    var mediador = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediador.Send(new ComandoEnviarRecordatorioCitas());
                }

                // Esperar una hora antes de volver a verificar
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

    }
}
