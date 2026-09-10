using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Patients.Commands.DeletePatient;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Patients
{
    public class DeletePatientUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly DeletePatientHandler _handler;
        private readonly DeletePatientCommandValidator _validator;
        private readonly ILogger<DeletePatientHandler> _logger;

        public DeletePatientUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<DeletePatientHandler>>();
            _validator = new DeletePatientCommandValidator();

            // Default: no appointments reference anything. Tests that need the conflict
            // path override this. Build the mock DbSet into a local FIRST —
            // BuildMockDbSet() calls .Returns() internally, which would clobber
            // NSubstitute's pending call if it ran inside _db.Appointments.Returns(...).
            var noAppointments = new List<Appointment>().BuildMockDbSet();
            _db.Appointments.Returns(noAppointments);

            _handler = new DeletePatientHandler(_db, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_PatientExists_RemovesPatientAndPersists()
        {
            // Arrange
            var patient = Patient.Create("Patient A", "patient@test.com").Value;
            var command = new DeletePatientCommand { Id = patient.Id };

            var dbSet = new List<Patient> { patient }.BuildMockDbSet();
            _db.Patients.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            dbSet.Received(1).Remove(patient);
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_PatientHasAppointments_ReturnsFailureConflict()
        {
            // Arrange
            var patient = Patient.Create("Patient A", "patient@test.com").Value;
            var command = new DeletePatientCommand { Id = patient.Id };

            var patientDbSet = new List<Patient> { patient }.BuildMockDbSet();
            _db.Patients.Returns(patientDbSet);

            var start = DateTime.UtcNow.AddDays(1);
            var appointment = Appointment.Create(
                patient.Id, Guid.NewGuid(), Guid.NewGuid(),
                start, start.AddHours(1), DateTime.UtcNow).Value;

            var appointmentDbSet = new List<Appointment> { appointment }.BuildMockDbSet();
            _db.Appointments.Returns(appointmentDbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Patient.HasAppointmentsConflict);
            patientDbSet.DidNotReceive().Remove(Arg.Any<Patient>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_PatientNotFound_ReturnsFailureNotFound()
        {
            // Arrange
            var command = new DeletePatientCommand { Id = Guid.NewGuid() };

            var dbSet = new List<Patient>().BuildMockDbSet();
            _db.Patients.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Patient.NotFound);
            dbSet.DidNotReceive().Remove(Arg.Any<Patient>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }


        // The validator is an external component; validation is no longer done inside the Handler
        // but through an external validator injected via the "ValidationBehavior" class

        [Fact]
        public void Validate_EmptyId_GeneratesValidationError()
        {
            // Arrange
            var command = new DeletePatientCommand { Id = Guid.Empty };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id);
        }

        [Fact]
        public void Validate_ValidId_GeneratesNoValidationError()
        {
            // Arrange
            var command = new DeletePatientCommand { Id = Guid.NewGuid() };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Id);
        }
    }
}