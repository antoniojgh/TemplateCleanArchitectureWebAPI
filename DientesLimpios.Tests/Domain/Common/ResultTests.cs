using System.Reflection;
using DientesLimpios.Domain.Common.ResultPattern;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.Common
{
    public class ResultTests
    {
        [Fact]
        public void ResultOfT_DeclaresNoImplicitConversions()
        {
            // Arrange & Act
            var conversions = typeof(Result<>)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "op_Implicit")
                .ToList();

            // Assert
            conversions.Should().BeEmpty(
                "a conversion that can silently turn a value into a failure hides the failure at the return site");
        }
    }
}
