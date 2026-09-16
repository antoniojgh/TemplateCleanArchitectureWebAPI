using System.Collections.Concurrent;
using DientesLimpios.Domain.Common;

namespace DientesLimpios.Application.Utilities.Mediator
{
    public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
    {
        private static readonly ConcurrentDictionary<Type, DomainEventHandlerWrapper> Wrappers = new();

        public Task Dispatch(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            var wrapper = Wrappers.GetOrAdd(domainEvent.GetType(), static eventType =>
                (DomainEventHandlerWrapper)Activator.CreateInstance(
                    typeof(DomainEventHandlerWrapper<>).MakeGenericType(eventType))!);

            return wrapper.Handle(domainEvent, serviceProvider, cancellationToken);
        }
    }
}