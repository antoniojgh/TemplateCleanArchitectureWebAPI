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

        [Theory]
        [InlineData("@")]
        [InlineData("a@")]
        [InlineData("@b")]
        [InlineData("a@@b.com")]
        [InlineData("Foo <a@b.com>")]     // a display name is not an address
        public void Create_DegenerateEmail_ReturnsFailureInvalidFormat(string email)
        {
            var result = Email.Create(email);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
        }

        [Theory]
        [InlineData("  ana@example.com  ", "ana@example.com")]
        [InlineData("Ana@Example.COM", "ana@example.com")]
        public void Create_EmailNeedingNormalisation_TrimsAndLowercases(string input, string expected)
        {
            Email.Create(input).Value.Value.Should().Be(expected);
        }

        [Fact]
        public void Create_EmailsDifferingOnlyInCase_AreEqual()
        {
            Email.Create("Ana@Example.com").Value
                .Should().Be(Email.Create("ana@example.com").Value);
        }

        [Fact]
        public void Create_EmailLongerThanMaxLength_ReturnsFailureTooLong()
        {
            var email = new string('a', Email.MaxLength) + "@example.com";

            var result = Email.Create(email);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.TooLong);
        }
    }
}
