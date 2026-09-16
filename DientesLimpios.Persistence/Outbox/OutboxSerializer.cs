using System.Collections.Frozen;
using System.Text.Json;
using DientesLimpios.Domain.Common;

namespace DientesLimpios.Persistence.Outbox
{
    // Rows store the event's type name, resolved only against concrete IDomainEvent types
    // in the Domain assembly. An AssemblyQualifiedName would embed the assembly version,
    // which breaks every stored row on a version bump, and would let a string read from
    // the database choose which type gets instantiated.
    public static class OutboxSerializer
    {
        // Two events with the same name in different namespaces make this throw at startup,
        // which is the failure you want.
        private static readonly FrozenDictionary<string, Type> EventTypes =
            typeof(IDomainEvent).Assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IDomainEvent)))
                .ToFrozenDictionary(t => t.Name, StringComparer.Ordinal);

        public static OutboxMessage ToOutboxMessage(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);
            var type = domainEvent.GetType();

            return new OutboxMessage(domainEvent.EventId, type.Name,
                JsonSerializer.Serialize(domainEvent, type), domainEvent.OccurredOnUtc);
        }

        // Null when the type is unknown, for example a row written by a newer deployment.
        public static IDomainEvent? ToDomainEvent(OutboxMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            return EventTypes.TryGetValue(message.Type, out var type)
                ? (IDomainEvent?)JsonSerializer.Deserialize(message.Payload, type)
                : null;
        }
    }
}
