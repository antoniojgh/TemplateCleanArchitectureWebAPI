using DientesLimpios.Application.UseCases.Appointments.Commands.RescheduleAppointment;
using FluentAssertions;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Appointments
{
    public class RescheduleAppointmentCommandValidatorTests
    {
        private static readonly DateTimeOffset Now = new(2030, 1, 1, 10, 0, 0, TimeSpan.Zero);

        private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

        public RescheduleAppointmentCommandValidatorTests()
        {
            _timeProvider.GetUtcNow().Returns(Now);
        }

        [Fact]
        public void Validate_FutureSlot_IsValid()
        {
            // Arrange
            var validator = new RescheduleAppointmentCommandValidator(_timeProvider);
            var command = Command(Now.UtcDateTime.AddHours(1), Now.UtcDateTime.AddHours(2));

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_StartDateBeforeClockNow_IsInvalid()
        {
            // Arrange — "now" is 2030, so the real clock would wrongly accept a 2029 date.
            var validator = new RescheduleAppointmentCommandValidator(_timeProvider);
            var command = Command(Now.UtcDateTime.AddDays(-1), Now.UtcDateTime.AddDays(-1).AddHours(1));

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "The start date cannot be in the past");
        }

        [Fact]
        public void Validate_StartDateNotBeforeEndDate_IsInvalid()
        {
            // Arrange
            var validator = new RescheduleAppointmentCommandValidator(_timeProvider);
            var start = Now.UtcDateTime.AddHours(2);
            var command = Command(start, start);

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "The start date must be earlier than the end date");
        }

        [Fact]
        public void Validate_ClockMovesAfterConstruction_UsesTheTimeOfValidation()
        {
            // Arrange — build the validator, then let time pass before validating.
            var validator = new RescheduleAppointmentCommandValidator(_timeProvider);
            _timeProvider.GetUtcNow().Returns(Now.AddHours(2));

            // Valid at construction time, already in the past at validation time.
            var command = Command(Now.UtcDateTime.AddHours(1), Now.UtcDateTime.AddHours(3));

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        private static RescheduleAppointmentCommand Command(DateTime start, DateTime end) => new()
        {
            Id = Guid.NewGuid(),
            StartDate = start,
            EndDate = end
        };
    }
}
