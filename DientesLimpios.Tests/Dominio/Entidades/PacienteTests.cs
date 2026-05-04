using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class PacienteTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Crear_NombreInvalido_RetornaFailureNombreObligatorio(string? nombre)
        {
            // Act
            var result = Paciente.Crear(nombre!, "felipe@ejemplo.com");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Paciente.NombreObligatorio);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Crear_EmailInvalido_RetornaFailureEmailVacio(string? email)
        {
            // Act
            var result = Paciente.Crear("Felipe", email!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Vacio);
        }

        [Fact]
        public void Crear_EmailFormatoInvalido_RetornaFailureFormatoInvalido()
        {
            // Act
            var result = Paciente.Crear("Felipe", "EmailInvalido");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.FormatoInvalido);
        }

        [Fact]
        public void Crear_PacienteValido_CreaInstanciaCorrecta()
        {
            // Act
            var result = Paciente.Crear("Felipe", "felipe@ejemplo.com");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var paciente = result.Value;

            paciente.Nombre.Should().Be("Felipe");
            paciente.Email.Valor.Should().Be("felipe@ejemplo.com");
            paciente.Id.Should().NotBeEmpty();
        }
    }
}
