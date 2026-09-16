using System.Data;
using DientesLimpios.Application.Utilities.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Persistence.Outbox
{
    public sealed class OutboxProcessor(DientesLimpiosDbContext context, IServiceScopeFactory scopeFactory, TimeProvider timeProvider, ILogger<OutboxProcessor> logger)
    {
        public const int BatchSize = 20;
        public const int MaxAttempts = 5;

        // Returns how many messages were picked up, so the caller can poll again at once
        // when the batch was full.
        public async Task<int> ProcessBatch(CancellationToken cancellationToken)
        {
            // The row locks from LockNextOutboxBatch last exactly as long as this transaction. It
            // stays open until every message is marked processed or failed, so another instance
            // cannot pick these rows up in the meantime.
            await using var transaction = await context.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var messages = await context.LockNextOutboxBatch(BatchSize, MaxAttempts, cancellationToken);

            foreach (var message in messages)
            {
                await Process(message, cancellationToken);
            }

            // One save and commit for the batch. Saving each message separately would not make it
            // durable any earlier inside the transaction. If the process dies before the commit, the
            // whole batch is delivered again. Idempotent handlers make that safe; the email handler
            // skips appointments whose ConfirmationSentAtUtc is already set.
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return messages.Count;
        }

        private async Task Process(OutboxMessage message, CancellationToken cancellationToken)
        {
            var domainEvent = OutboxSerializer.ToDomainEvent(message);

            if (domainEvent is null)
            {
                // Retried rather than dropped: after a rollback to an older deployment,
                // a newer version may be the one that knows this type.
                message.MarkFailed($"Unknown event type '{message.Type}'.");
                logger.LogError("Outbox message {MessageId} has unknown type {EventType}.", message.Id, message.Type);
                return;
            }

            try
            {
                // One scope per message: handlers get their own DbContext, so a handler that
                // fails partway cannot leave tracked changes for the next message to save.
                await using var scope = scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

                await dispatcher.Dispatch(domainEvent, cancellationToken);

                message.MarkProcessed(timeProvider.GetUtcNow().UtcDateTime);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Shutdown is not a failed attempt, hence the filter.
                message.MarkFailed($"{ex.GetType().Name}: {ex.Message}");
                logger.LogError(ex, "Outbox message {MessageId} ({EventType}) failed on attempt {Attempt}.",
                    message.Id, message.Type, message.AttemptCount);
            }
        }
    }
}