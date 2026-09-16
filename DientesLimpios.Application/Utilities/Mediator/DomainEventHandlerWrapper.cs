using DientesLimpios.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Application.Utilities.Mediator
{
    // Lets the dispatcher call IDomainEventHandler<TEvent> without knowing TEvent at
    // compile time. Reflection is used once per event type, to build the closed wrapper.
    internal abstract class DomainEventHandlerWrapper
    {
        public abstract Task Handle(IDomainEvent domainEvent, IServiceProvider serviceProvider,
                                    CancellationToken cancellationToken);
    }

    internal sealed class DomainEventHandlerWrapper<TEvent> : DomainEventHandlerWrapper
        where TEvent : IDomainEvent
    {
        public override async Task Handle(IDomainEvent domainEvent, IServiceProvider serviceProvider,
                                          CancellationToken cancellationToken)
        {
            foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
            {
                await handler.Handle((TEvent)domainEvent, cancellationToken);
            }
        }
    }
}
