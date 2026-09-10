using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Dentists.Commands.DeleteDentist;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Dentists
{
    public class DeleteDentistUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly DeleteDentistHandler _handler;
        private readonly DeleteDentistCommandValidator _validator;
        private readonly ILogger<DeleteDentistHandler> _logger;

        public DeleteDentistUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<DeleteDentistHandler>>();
            _validator = new DeleteDentistCommandValidator();

            // Default: no appointments reference anything. Build into a local first —
            // BuildMockDbSet() calls .Returns() internally.
            var noAppointments = new List<Appointment>().BuildMockDbSet();
            _db.Appointments.Returns(noAppointments);

            _handler = new DeleteDentistHandler(_db, _logger);
        }

        [Fact]
        public async Task Handle_DentistExists_RemovesDentistAndPersists()
        {
            // Arrange
            var dentist = Dentist.Create("Dentist A", "dentist@test.com").Value;
            var command = new DeleteDentistCommand { Id = dentist.Id };

            var dbSet = new List<Dentist> { dentist }.BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            dbSet.Received(1).Remove(dentist);
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DentistHasAppointments_ReturnsFailureConflict()
        {
            // Arrange
            var dentist = Dentist.Create("Dentist A", "dentist@test.com").Value;
            var command = new DeleteDentistCommand { Id = dentist.Id };

            var dentistDbSet = new List<Dentist> { dentist }.BuildMockDbSet();
            _db.Dentists.Returns(dentistDbSet);

            var start = DateTime.UtcNow.AddDays(1);
            var appointment = Appointment.Create(
                Guid.NewGuid(), dentist.Id, Guid.NewGuid(),
                start, start.AddHours(1), DateTime.UtcNow).Value;

            var appointmentDbSet = new List<Appointment> { appointment }.BuildMockDbSet();
            _db.Appointments.Returns(appointmentDbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Dentist.HasAppointmentsConflict);
            dentistDbSet.DidNotReceive().Remove(Arg.Any<Dentist>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DentistNotFound_ReturnsFailureNotFound()
        {
            // Arrange
            var command = new DeleteDentistCommand { Id = Guid.NewGuid() };

            var dbSet = new List<Dentist>().BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Dentist.NotFound);
            dbSet.DidNotReceive().Remove(Arg.Any<Dentist>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public void Validate_EmptyId_GeneratesValidationError()
        {
            // Arrange
            var command = new DeleteDentistCommand { Id = Guid.Empty };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id);
        }

        [Fact]
        public void Validate_ValidId_GeneratesNoValidationError()
        {
            // Arrange
            var command = new DeleteDentistCommand { Id = Guid.NewGuid() };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Id);
        }
    }
}
