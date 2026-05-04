using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class DentistaTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Crear_NombreInvalido_RetornaFailureNombreObligatorio(string? nombre)
        {
            // Act
            var result = Dentista.Crear(nombre!, "felipe@ejemplo.com");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Dentista.NombreObligatorio);

        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Crear_EmailInvalido_RetornaFailureEmailVacio(string? email)
        {
            // Act
            var result = Dentista.Crear("Felipe", email!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Vacio);
        }

        [Fact]
        public void Crear_EmailFormatoInvalido_RetornaFailureFormatoInvalido()
        {
            // Act
            var result = Dentista.Crear("Felipe", "EmailInvalido");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.FormatoInvalido);
        }

        [Fact]
        public void Crear_DentistaValido_CreaInstanciaCorrecta()
        {
            // Act
            var result = Dentista.Crear("Felipe", "felipe@ejemplo.com");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var dentista = result.Value;

            dentista.Nombre.Should().Be("Felipe");
            dentista.Email.Valor.Should().Be("felipe@ejemplo.com");
            dentista.Id.Should().NotBeEmpty();
        }
    }
}
