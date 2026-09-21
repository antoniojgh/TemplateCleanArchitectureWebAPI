using System.Reflection;
using DientesLimpios.Domain.Common;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.Common
{
    public class AggregateRootTests
    {
        [Fact]
        public void AggregateRoot_ExposesNoPubliclyWritableState()
        {
            // Arrange & Act
            var writable = typeof(AggregateRoot)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.SetMethod is { IsPublic: true })
                .Select(p => p.Name)
                .ToList();

            // Assert
            writable.Should().BeEmpty(
                "audit data is written by the persistence interceptor; holding an aggregate " +
                "reference must not be enough to rewrite CreatedDate");
        }

        [Fact]
        public void IAuditable_ExposesNoSetters()
        {
            // Arrange & Act
            var settable = typeof(IAuditable)
                .GetProperties()
                .Where(p => p.SetMethod is not null)
                .Select(p => p.Name)
                .ToList();

            // Assert
            settable.Should().BeEmpty(
                "a setter on the interface would let any caller cast to IAuditable and forge the audit trail");
        }
    }
}