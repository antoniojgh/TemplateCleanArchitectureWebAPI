using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class ConsultorioTests
    {

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Crear_NombreInvalido_RetornaFailureNombreObligatorio(string? nombre)
        {
            // Act
            var result = Consultorio.Crear(nombre!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Consultorio.NombreObligatorio);
        }

        [Fact]
        public void Crear_NombreValido_CreaInstanciaCorrecta()
        {
            // Act
            var result = Consultorio.Crear("Consultorio Central");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var consultorio = result.Value;

            consultorio.Nombre.Should().Be("Consultorio Central");
            consultorio.Id.Should().NotBeEmpty();
        }
    }
}
