using DientesLimpios.Domain.Events;
using DientesLimpios.Persistence;
using DientesLimpios.Persistence.Outbox;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.IntegrationTests
{
    [Collection(IntegrationCollection.Name)]
    public sealed class OutboxProcessorTests(IntegrationTestFactory factory)
    {
        [Fact]
        public async Task LockNextOutboxBatch_RowLockedByAnotherInstance_IsSkipped()
        {
            // Arrange: at least two unprocessed messages, so a second instance has one to take.
            await AddOutboxMessagesAsync(count: 2);

            using var scopeA = factory.Services.CreateScope();
            using var scopeB = factory.Services.CreateScope();
            var instanceA = scopeA.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            var instanceB = scopeB.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            await using var transactionA = await instanceA.Database.BeginTransactionAsync();
            var lockedByA = await instanceA.LockNextOutboxBatch(1, OutboxProcessor.MaxAttempts, CancellationToken.None);

            // Act: a second instance polls while A still holds its transaction open.
            await using var transactionB = await instanceB.Database.BeginTransactionAsync();
            var lockedByB = await instanceB.LockNextOutboxBatch(1, OutboxProcessor.MaxAttempts, CancellationToken.None);

            // Assert
            lockedByA.Should().ContainSingle();
            lockedByB.Should().ContainSingle();
            lockedByB[0].Id.Should().NotBe(lockedByA[0].Id);
        }

        [Fact]
        public async Task LockNextOutboxBatch_WithoutTransaction_Throws()
        {
            // Arrange
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            // Act
            var act = () => db?.LockNextOutboxBatch(1, OutboxProcessor.MaxAttempts, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // The events point at appointments that don't exist. When another test drains the
        // outbox, the handler logs "not found" and the message is marked processed, so these
        // rows don't affect other tests.
        private async Task AddOutboxMessagesAsync(int count)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            // The app's clock, so these rows are ordered consistently with the ones it writes.
            var now = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;

            for (var i = 0; i < count; i++)
            {
                var start = DateTime.UtcNow.AddDays(1);
                var domainEvent = new AppointmentCreatedEvent(Guid.CreateVersion7(), Guid.CreateVersion7(),
                    Guid.CreateVersion7(), Guid.CreateVersion7(), start, start.AddHours(1), now);

                db.OutboxMessages.Add(OutboxSerializer.ToOutboxMessage(domainEvent));
            }

            await db.SaveChangesAsync();
        }
    }
}
