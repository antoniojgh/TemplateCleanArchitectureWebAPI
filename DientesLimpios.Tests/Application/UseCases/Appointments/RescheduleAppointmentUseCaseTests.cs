using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.UseCases.Appointments.Commands.RescheduleAppointment;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Appointments
{
    public class RescheduleAppointmentUseCaseTests
    {
        private static readonly DateTime NowUtc = new(2026, 9, 16, 6, 0, 0, DateTimeKind.Utc);

        private readonly IAppointmentRepository _repository = Substitute.For<IAppointmentRepository>();
        private readonly RescheduleAppointmentHandler _handler;

        public RescheduleAppointmentUseCaseTests()
        {
            _handler = new RescheduleAppointmentHandler(
                _repository, Substitute.For<ILogger<RescheduleAppointmentHandler>>());
        }

        [Fact]
        public async Task Handle_ExistingAppointment_ReschedulesThroughTheRepository()
        {
            // Arrange
            var appointment = ScheduledAppointment();
            var newStart = NowUtc.AddDays(5);
            var newEnd = newStart.AddHours(1);

            _repository.GetById(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
            _repository.RescheduleIfNoOverlap(appointment, newStart, newEnd, Arg.Any<CancellationToken>())
                       .Returns(Result.Success(appointment.Id));

            // Act
            var result = await _handler.Handle(Command(appointment.Id, newStart, newEnd), CancellationToken.None);

            // Assert — the loaded aggregate and the requested slot reach the repository unchanged.
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(appointment.Id);
            await _repository.Received(1).RescheduleIfNoOverlap(
                appointment, newStart, newEnd, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_AppointmentNotFound_ReturnsFailureNotFound()
        {
            // Arrange
            _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

            // Act
            var result = await _handler.Handle(
                Command(Guid.CreateVersion7(), NowUtc.AddDays(5), NowUtc.AddDays(5).AddHours(1)),
                CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Appointment.NotFound);
            await _repository.DidNotReceiveWithAnyArgs().RescheduleIfNoOverlap(default!, default, default, default);
        }

        [Fact]
        public async Task Handle_RepositoryReportsOverlap_ReturnsFailureOverlapping()
        {
            // Arrange
            var appointment = ScheduledAppointment();
            _repository.GetById(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
            _repository.RescheduleIfNoOverlap(appointment, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
                       .Returns(Result.Failure<Guid>(DomainErrors.Appointment.Overlapping));

            // Act
            var result = await _handler.Handle(
                Command(appointment.Id, NowUtc.AddDays(5), NowUtc.AddDays(5).AddHours(1)),
                CancellationToken.None);

            // Assert — the repository's error is passed through, so the API answers 409.
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Appointment.Overlapping);
        }

        private static Appointment ScheduledAppointment()
        {
            var start = NowUtc.AddDays(2);

            return Appointment.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
                                      start, start.AddHours(1), NowUtc).Value;
        }

        private static RescheduleAppointmentCommand Command(Guid id, DateTime start, DateTime end) => new()
        {
            Id = id,
            StartDate = start,
            EndDate = end
        };
    }
}
