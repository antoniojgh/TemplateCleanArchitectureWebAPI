using DientesLimpios.Domain.Events;
using DientesLimpios.Persistence.Outbox;
using FluentAssertions;

namespace DientesLimpios.Tests.Persistence.Outbox
{
    public class OutboxSerializerTests
    {
        [Fact]
        public void RoundTrip_AppointmentCreatedEvent_PreservesEventIdAndOccurredOnUtc()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(1);
            var original = new AppointmentCreatedEvent(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                start, start.AddHours(1));

            // Act
            var message = OutboxSerializer.ToOutboxMessage(original);
            var restored = OutboxSerializer.ToDomainEvent(message);

            // Assert
            // Record equality compares every property, including EventId and OccurredOnUtc.
            restored.Should().BeOfType<AppointmentCreatedEvent>()
                .Which.Should().Be(original);
            message.Id.Should().Be(original.EventId);
        }
    }
}