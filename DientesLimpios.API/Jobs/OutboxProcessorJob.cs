using DientesLimpios.Persistence.Outbox;

namespace DientesLimpios.API.Jobs
{
    // Delivers stored domain events outside the HTTP request that raised them.
    public class OutboxProcessorJob(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorJob> logger) : BackgroundService
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(180);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(PollingInterval);

            do
            {
                try
                {
                    int processed;
                    do
                    {
                        await using var scope = scopeFactory.CreateAsyncScope();
                        var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                        processed = await processor.ProcessBatch(stoppingToken);
                    }
                    while (processed == OutboxProcessor.BatchSize);   // drain a backlog without waiting
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    // Database unreachable, etc. Keep the loop alive; the next tick retries.
                    logger.LogError(ex, "Outbox processing cycle failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
