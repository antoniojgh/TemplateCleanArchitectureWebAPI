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
                start, start.AddHours(1), new DateTime(2026, 9, 16, 8, 30, 0, DateTimeKind.Utc));

            // Act
            var message = OutboxSerializer.ToOutboxMessage(original);
            var restored = OutboxSerializer.ToDomainEvent(message);

            // Assert
            // Record equality compares every property, including EventId and OccurredOnUtc.
            restored.Should().BeOfType<AppointmentCreatedEvent>()
                .Which.Should().Be(original);
            message.Id.Should().Be(original.EventId);
        }

        [Fact]
        public void RoundTrip_AppointmentRescheduledEvent_PreservesEveryProperty()
        {
            // Arrange
            var newStart = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
            var original = new AppointmentRescheduledEvent(
                Guid.NewGuid(), Guid.NewGuid(), newStart, newStart.AddHours(1),
                new DateTime(2026, 9, 16, 8, 30, 0, DateTimeKind.Utc));

            // Act
            var message = OutboxSerializer.ToOutboxMessage(original);
            var restored = OutboxSerializer.ToDomainEvent(message);

            // Assert — the type resolves by its short name, and nothing comes back with a fresh value.
            message.Type.Should().Be(nameof(AppointmentRescheduledEvent));
            restored.Should().BeOfType<AppointmentRescheduledEvent>()
                .Which.Should().Be(original);
            message.Id.Should().Be(original.EventId);
        }
    }
}