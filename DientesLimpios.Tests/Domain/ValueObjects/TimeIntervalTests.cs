using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.ValueObjects;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.ValueObjects
{
    public class TimeIntervalTests
    {
        [Fact]
        public void Create_StartAfterEnd_ReturnsFailureStartGreaterThanOrEqualToEnd()
        {
            var now = DateTime.UtcNow;

            // Act
            var result = TimeInterval.Create(now, now.AddDays(-1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.TimeInterval.StartGreaterThanOrEqualToEnd);
        }

        [Fact]
        public void Create_StartEqualToEnd_ReturnsFailureStartGreaterThanOrEqualToEnd()
        {
            var now = DateTime.UtcNow;

            // Act
            var result = TimeInterval.Create(now, now);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.TimeInterval.StartGreaterThanOrEqualToEnd);
        }

        [Fact]
        public void Create_ValidParameters_CreatesInstanceCorrectly()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var end = start.AddMinutes(30);

            // Act
            var result = TimeInterval.Create(start, end);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var interval = result.Value;
            interval.Start.Should().Be(start);
            interval.End.Should().Be(end);
        }
    }
}
