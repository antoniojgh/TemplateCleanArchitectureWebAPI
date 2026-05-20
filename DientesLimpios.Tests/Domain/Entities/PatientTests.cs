using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.Entities
{
    public class PatientTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_InvalidName_ReturnsFailureNameRequired(string? name)
        {
            // Act
            var result = Patient.Create(name!, "felipe@ejemplo.com");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Patient.NameRequired);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_InvalidEmail_ReturnsFailureEmailEmpty(string? email)
        {
            // Act
            var result = Patient.Create("Felipe", email!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Empty);
        }

        [Fact]
        public void Create_InvalidEmailFormat_ReturnsFailureInvalidFormat()
        {
            // Act
            var result = Patient.Create("Felipe", "EmailInvalido");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
        }

        [Fact]
        public void Create_ValidPatient_CreatesInstanceCorrectly()
        {
            // Act
            var result = Patient.Create("Felipe", "felipe@ejemplo.com");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var patient = result.Value;

            patient.Name.Should().Be("Felipe");
            patient.Email.Value.Should().Be("felipe@ejemplo.com");
            patient.Id.Should().NotBeEmpty();
        }
    }
}
