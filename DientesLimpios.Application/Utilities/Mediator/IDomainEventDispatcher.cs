using DientesLimpios.Domain.Common;

namespace DientesLimpios.Application.Utilities.Mediator
{
    public interface IDomainEventDispatcher
    {
        Task Dispatch(IDomainEvent domainEvent, CancellationToken cancellationToken);
    }

}
