using DientesLimpios.Dominio.Errores;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.ObjetosDeValor
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
            var result = Email.Crear(email!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Vacio);
        }

        [Theory]
        [InlineData("EmailInvalido")]      // no @
        [InlineData("sin-arroba.com")]     // no @
        public void Crear_EmailSinArroba_RetornaFailureFormatoInvalido(string email)
        {
            var result = Email.Crear(email);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.FormatoInvalido);
        }

        [Theory]
        [InlineData("@")]
        [InlineData("a@")]
        [InlineData("@b")]
        public void Crear_EmailDegenerado_RetornaSuccess_LimitacionConocida(string email)
        {
            // The current implementation only checks for '@' presence,
            // not full RFC 5321 validity. These pass — by design, for now.
            var result = Email.Crear(email);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Crear_EmailValido_CreaInstanciaCorrecta()
        {
            // Act
            var result = Email.Crear("felipe@ejemplo.com");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var email = result.Value;
            email.Valor.Should().Be("felipe@ejemplo.com");
        }
    }
}
