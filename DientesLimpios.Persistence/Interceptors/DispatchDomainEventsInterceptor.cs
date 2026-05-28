using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;


namespace DientesLimpios.Persistence.Interceptors
{
    public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceScopeFactory _scopeFactory;

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

            // Create a fresh DI scope so handlers can resolve their own scoped services
            // (including repositories with their own DbContext state) without conflicting
            // with the currently saving DbContext.
            using var scope = _scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

            foreach (var domainEvent in events)
            {
                await dispatcher.Dispatch(domainEvent, cancellationToken);
            }

            return result;
        }
    }

}
