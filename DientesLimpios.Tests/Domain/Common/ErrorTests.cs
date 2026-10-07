using DientesLimpios.Domain.Common.ResultPattern;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.Common
{
    public class ErrorTests
    {
        [Fact]
        public void TwoErrorsWithSameCodeAndMessage_AreEqual()
        {
            var a = new Error("X.Y", "Same message");
            var b = new Error("X.Y", "Same message");

            a.Should().Be(b);
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }
    }
}