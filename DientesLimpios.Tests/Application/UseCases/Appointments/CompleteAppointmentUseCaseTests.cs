using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.UseCases.Appointments.Commands.CompleteAppointment;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Enums;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DientesLimpios.Tests.Application.UseCases.Appointments
{
    public class CompleteAppointmentUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly IAppointmentRepository _repository;

        private readonly TimeProvider _timeProvider;
        private readonly ILogger<CompleteAppointmentHandler> _logger;
        private readonly CompleteAppointmentHandler _handler;

        public CompleteAppointmentUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _repository = Substitute.For<IAppointmentRepository>();
            _logger = Substitute.For<ILogger<CompleteAppointmentHandler>>();
            _timeProvider = Substitute.For<TimeProvider>();

            _handler = new CompleteAppointmentHandler(_db, _repository, _timeProvider, _logger);
        }

        [Fact]
        public async Task Handle_ScheduledAppointment_CompletesAndPersists()
        {
            // Arrange
            var appointment = ScheduledAppointment();
            _repository.GetById(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

            // Act
            var result = await _handler.Handle(
                new CompleteAppointmentCommand { Id = appointment.Id }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            appointment.Status.Should().Be(AppointmentStatus.Completed);
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_AppointmentChangedConcurrently_ReturnsConcurrencyConflict()
        {
            // Arrange — someone else changed the row between our read and our write, so the
            // concurrency token makes the database refuse this update.
            var appointment = ScheduledAppointment();
            _repository.GetById(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

            _db.SaveChangesAsync(Arg.Any<CancellationToken>())
               .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _handler.Handle(
                new CompleteAppointmentCommand { Id = appointment.Id }, CancellationToken.None);

            // Assert — a 409-mapped outcome, not an exception escaping the handler.
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Concurrency.Conflict);
        }

        [Fact]
        public async Task Handle_AppointmentNotFound_ReturnsFailureNotFound()
        {
            // Arrange
            _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

            // Act
            var result = await _handler.Handle(
                new CompleteAppointmentCommand { Id = Guid.CreateVersion7() }, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Appointment.NotFound);
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        private static Appointment ScheduledAppointment()
        {
            var nowUtc = DateTime.UtcNow;
            var start = nowUtc.AddDays(2);

            return Appointment.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
                                      start, start.AddHours(1), nowUtc).Value;
        }
    }
}