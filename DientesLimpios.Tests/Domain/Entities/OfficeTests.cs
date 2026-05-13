using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.Entities
{
    public class OfficeTests
    {

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Crear_NombreInvalido_RetornaFailureNameRequired(string? name)
        {
            // Act
            var result = Office.Create(name!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NameRequired);
        }

        [Fact]
        public void Crear_NombreValido_CreaInstanciaCorrecta()
        {
            // Act
            var result = Office.Create("Office Central");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var office = result.Value;

            office.Name.Should().Be("Office Central");
            office.Id.Should().NotBeEmpty();
        }
    }
}
