using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.ValueObjects;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.ValueObjects
{
    public class EmailTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_InvalidEmail_ReturnsFailureEmailEmpty(string? email)
        {
            // Act
            var result = Email.Create(email!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Empty);
        }

        [Theory]
        [InlineData("EmailInvalido")]      // no @
        [InlineData("sin-arroba.com")]     // no @
        public void Create_EmailWithoutAtSign_ReturnsFailureInvalidFormat(string email)
        {
            var result = Email.Create(email);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
        }

        [Theory]
        [InlineData("@")]
        [InlineData("a@")]
        [InlineData("@b")]
        public void Create_DegenerateEmail_ReturnsSuccess_KnownLimitation(string email)
        {
            // The current implementation only checks for '@' presence,
            // not full RFC 5321 validity. These pass — by design, for now.
            var result = Email.Create(email);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_ValidEmail_CreatesInstanceCorrectly()
        {
            // Act
            var result = Email.Create("felipe@ejemplo.com");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var email = result.Value;
            email.Value.Should().Be("felipe@ejemplo.com");
        }
    }
}
