using System.Data.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;


namespace DientesLimpios.Persistence.Interceptors
{
    // Dispatches domain events raised during a SaveChanges cycle.
    //
    // When the caller owns an explicit transaction the write is not durable yet, so the
    // events are held until it commits. Dispatching them earlier would announce a change
    // that may still roll back, and would run the handlers — which do real I/O, such as
    // sending a confirmation email — while the transaction and all of its locks are still
    // open. AppointmentRepository.AddIfNoOverlap depends on that: it holds an exclusive
    // lock on the dentist, and an SMTP round trip inside it would serialise every booking
    // for that dentist behind an email.
    public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor, IDbTransactionInterceptor
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // The interceptor is registered scoped, so this state belongs to a single
        // DbContext instance and never crosses requests.
        private readonly List<IDomainEvent> _eventsAwaitingCommit = [];

        public DispatchDomainEventsInterceptor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
                                                                CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null)
                return result;

            // Collect entities that raised events during this SaveChanges cycle.
            var entitiesWithEvents = eventData.Context.ChangeTracker
                                    .Entries<AggregateRoot>()
                                    .Where(e => e.Entity.DomainEvents.Count > 0)
                                    .Select(e => e.Entity)
                                    .ToList();

            // Snapshot the events, then clear them so they are not dispatched twice
            // if SaveChanges is called again within the same request.
            var events = entitiesWithEvents
                        .SelectMany(e => e.DomainEvents)
                        .ToList();

            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            if (events.Count == 0)
                return result;

            if (eventData.Context.Database.CurrentTransaction is not null)
            {
                _eventsAwaitingCommit.AddRange(events);
                return result;
            }

            await Dispatch(events, cancellationToken);

            return result;
        }

        public async Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
                                                     CancellationToken cancellationToken = default)
        {
            if (_eventsAwaitingCommit.Count == 0)
                return;

            var events = _eventsAwaitingCommit.ToList();
            _eventsAwaitingCommit.Clear();

            await Dispatch(events, cancellationToken);
        }

        public Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData,
                                                CancellationToken cancellationToken = default)
        {
            // The write never happened, so neither did the events.
            _eventsAwaitingCommit.Clear();
            return Task.CompletedTask;
        }

        // Disposing an uncommitted transaction rolls it back without raising the rollback
        // event, so this is the safety net that keeps abandoned events from leaking into
        // the next transaction on the same DbContext. After a commit the list is already
        // empty, so this is a no-op there.
        public void TransactionDisposed(DbTransaction transaction, TransactionEndEventData eventData)
        {
            _eventsAwaitingCommit.Clear();
        }

        private async Task Dispatch(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken)
        {
            // Create a fresh DI scope so handlers can resolve their own scoped services
            // (including repositories with their own DbContext state) without conflicting
            // with the currently saving DbContext.
            using var scope = _scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

            foreach (var domainEvent in events)
            {
                await dispatcher.Dispatch(domainEvent, cancellationToken);
            }
        }
    }
}