using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.ValueObjects;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.ValueObjects
{
    public class TimeIntervalTests
    {
        [Fact]
        public void Crear_FechaInicioPosteriorAFin_RetornaFailureStartGreaterThanOrEqualToEnd()
        {
            var ahora = DateTime.UtcNow;

            // Act
            var result = TimeInterval.Create(ahora, ahora.AddDays(-1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.TimeInterval.StartGreaterThanOrEqualToEnd);
        }

        [Fact]
        public void Crear_FechaInicioIgualAFin_RetornaFailureStartGreaterThanOrEqualToEnd()
        {
            var ahora = DateTime.UtcNow;

            // Act
            var result = TimeInterval.Create(ahora, ahora);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.TimeInterval.StartGreaterThanOrEqualToEnd);
        }

        [Fact]
        public void Crear_ParametrosCorrectos_CreaInstanciaCorrecta()
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
