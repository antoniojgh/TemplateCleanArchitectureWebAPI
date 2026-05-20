using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.Entities
{
    public class DentistTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_InvalidName_ReturnsFailureNameRequired(string? name)
        {
            // Act
            var result = Dentist.Create(name!, "felipe@ejemplo.com");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Dentist.NameRequired);

        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_InvalidEmail_ReturnsFailureEmailEmpty(string? email)
        {
            // Act
            var result = Dentist.Create("Felipe", email!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Empty);
        }

        [Fact]
        public void Create_InvalidEmailFormat_ReturnsFailureInvalidFormat()
        {
            // Act
            var result = Dentist.Create("Felipe", "EmailInvalido");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
        }

        [Fact]
        public void Create_ValidDentist_CreatesInstanceCorrectly()
        {
            // Act
            var result = Dentist.Create("Felipe", "felipe@ejemplo.com");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var dentist = result.Value;

            dentist.Name.Should().Be("Felipe");
            dentist.Email.Value.Should().Be("felipe@ejemplo.com");
            dentist.Id.Should().NotBeEmpty();
        }
    }
}
