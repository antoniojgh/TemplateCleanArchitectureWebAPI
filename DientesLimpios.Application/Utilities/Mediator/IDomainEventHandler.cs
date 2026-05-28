using DientesLimpios.Domain.Common;

namespace DientesLimpios.Application.Utilities.Mediator
{
    public interface IDomainEventHandler<in TEvent>
        where TEvent : IDomainEvent
    {
        Task Handle(TEvent domainEvent, CancellationToken cancellationToken);
    }

}
