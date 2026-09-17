using DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment;
using FluentAssertions;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Appointments
{
    public class CreateAppointmentCommandValidatorTests
    {
        private static readonly DateTimeOffset Now = new(2030, 1, 1, 10, 0, 0, TimeSpan.Zero);

        private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

        [Fact]
        public void Validate_StartDateBeforeClockNow_IsInvalid()
        {
            // Arrange — "now" is 2030, so the real clock would wrongly accept a 2029 date.
            _timeProvider.GetUtcNow().Returns(Now);
            var validator = new CreateAppointmentCommandValidator(_timeProvider);

            var command = CommandStartingAt(Now.UtcDateTime.AddDays(-1));

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "The start date cannot be in the past");
        }

        [Fact]
        public void Validate_StartDateAfterClockNow_IsValid()
        {
            // Arrange
            _timeProvider.GetUtcNow().Returns(Now);
            var validator = new CreateAppointmentCommandValidator(_timeProvider);

            var command = CommandStartingAt(Now.UtcDateTime.AddHours(1));

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ClockMovesAfterConstruction_UsesTheTimeOfValidation()
        {
            // Arrange — build the validator, then let time pass before validating.
            _timeProvider.GetUtcNow().Returns(Now);
            var validator = new CreateAppointmentCommandValidator(_timeProvider);

            _timeProvider.GetUtcNow().Returns(Now.AddHours(2));

            // Valid at construction time, already in the past at validation time.
            var command = CommandStartingAt(Now.UtcDateTime.AddHours(1));

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        private static CreateAppointmentCommand CommandStartingAt(DateTime start) => new()
        {
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            OfficeId = Guid.NewGuid(),
            StartDate = start,
            EndDate = start.AddHours(1)
        };
    }
}