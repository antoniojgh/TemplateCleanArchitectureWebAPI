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
        public void Create_InvalidName_ReturnsFailureNameRequired(string? name)
        {
            // Act
            var result = Office.Create(name!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NameRequired);
        }

        [Fact]
        public void Create_ValidName_CreatesInstanceCorrectly()
        {
            // Act
            var result = Office.Create("Office Central");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var office = result.Value;

            office.Name.Should().Be("Office Central");
            office.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Create_NameLongerThanMaxLength_ReturnsFailureNameTooLong()
        {
            var result = Office.Create(new string('a', Office.NameMaxLength + 1));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NameTooLong);
        }
    }
}
