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
        public void Crear_EmailInvalido_RetornaFailureEmailVacio(string? email)
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
        public void Crear_EmailSinArroba_RetornaFailureInvalidFormat(string email)
        {
            var result = Email.Create(email);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
        }

        [Theory]
        [InlineData("@")]
        [InlineData("a@")]
        [InlineData("@b")]
        public void Crear_EmailDegenerado_RetornaSuccess_LimitacionConocida(string email)
        {
            // The current implementation only checks for '@' presence,
            // not full RFC 5321 validity. These pass — by design, for now.
            var result = Email.Create(email);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Crear_EmailValido_CreaInstanciaCorrecta()
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
