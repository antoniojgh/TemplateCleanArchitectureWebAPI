using DientesLimpios.Domain.Common;
using DientesLimpios.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DientesLimpios.Persistence.Interceptors
{
    // Turns domain events into OutboxMessage rows inside the SaveChanges that persists the
    // aggregate, so the change and its events commit, or roll back, together.
    //
    // Nothing is dispatched here; OutboxProcessor delivers the rows after commit. Inside an
    // explicit transaction such as AppointmentRepository.AddIfNoOverlap the rows belong to
    // that transaction, so no event is observable before it commits and no handler I/O runs
    // while the dentist lock is held.
    public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
                InsertOutboxMessages(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // The previous interceptor overrode only the async method, so a synchronous
        // SaveChanges dropped events without any error.
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
                InsertOutboxMessages(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        private static void InsertOutboxMessages(DbContext context)
        {
            // Materialize before clearing: the events must be copied out of the aggregates
            // before ClearDomainEvents empties the lists that hold them.
            var aggregates = context.ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .Where(a => a.DomainEvents.Count > 0)
                .ToList();

            var messages = aggregates
                .SelectMany(a => a.DomainEvents)
                .Select(OutboxSerializer.ToOutboxMessage)
                .ToList();

            aggregates.ForEach(a => a.ClearDomainEvents());

            context.Set<OutboxMessage>().AddRange(messages);
        }
    }
}
