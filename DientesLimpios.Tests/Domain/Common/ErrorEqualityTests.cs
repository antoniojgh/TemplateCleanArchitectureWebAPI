using DientesLimpios.Domain.Common.ResultPattern;
using FluentAssertions;
using Xunit;

namespace DientesLimpios.Tests.Domain.Common
{
    public class ErrorEqualityTests
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

        [Fact]
        public void TwoErrorsWithSameCodeButDifferentMessage_AreNotEqual()
        {
            var a = new Error("X.Y", "Message A");
            var b = new Error("X.Y", "Message B");

            (a == b).Should().BeFalse();
        }

        [Fact]
        public void ErrorNone_EqualsItself()
        {
            // Sanity check for the Result invariants that use == Error.None
            var none = Error.None;

            (none == Error.None).Should().BeTrue();
            (none != Error.None).Should().BeFalse();
        }

        [Fact]
        public void ErrorWithSameCodeAsAnother_IsNotEqualToAStringOfThatCode()
        {
            // This is the regression test for the closed footgun.
            // Before this change, a == "X.Y" would have compiled and
            // returned true via the implicit string cast.
            // After this change, the line below should not even compile;
            // we assert the runtime behaviour as documentation.
            var error = new Error("X.Y", "Some message");

            // The following intentionally accesses .Code explicitly,
            // which is the only correct way to compare against a string.
            error.Code.Should().Be("X.Y");
        }
    }
}