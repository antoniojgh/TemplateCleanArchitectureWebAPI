using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.IntegrationTests
{
    [Collection(IntegrationCollection.Name)]
    public sealed class AuditingTests(IntegrationTestFactory factory)
    {
        [Fact]
        public void SaveChanges_SynchronousInsert_StampsCreationAudit()
        {
            // Arrange
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            var office = Office.Create("Sync Audit Office").Value;
            db.Offices.Add(office);

            // Act — the synchronous path, which the interceptor never intercepted.
            db.SaveChanges();

            // Assert
            office.CreatedDate.Should().NotBe(default);
            office.CreatedDate.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public async Task SaveChangesAsync_Update_StampsModificationAudit()
        {
            // Arrange
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            var office = Office.Create("Async Audit Office").Value;
            db.Offices.Add(office);
            await db.SaveChangesAsync();

            // Act
            office.UpdateName("Async Audit Office Renamed");
            await db.SaveChangesAsync();

            // Assert
            office.LastModifiedDate.Should().NotBeNull();
            office.LastModifiedDate!.Value.Should().BeOnOrAfter(office.CreatedDate);
        }
    }
}