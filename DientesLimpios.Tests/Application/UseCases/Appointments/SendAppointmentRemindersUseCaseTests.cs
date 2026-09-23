using DientesLimpios.Application.Configuration;
using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders;
using DientesLimpios.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Appointments
{
    public class SendAppointmentRemindersUseCaseTests
    {
        private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
        private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
        private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

        // One patient, dentist and office for every test: the interesting variable is when
        // the appointment starts, not who is in it.
        private readonly Patient _patient = Patient.Create("Ana", "ana@example.com").Value;
        private readonly Dentist _dentist = Dentist.Create("Claudia", "claudia@example.com").Value;
        private readonly Office _office = Office.Create("Main Office").Value;

        [Fact]
        public async Task Handle_InSummerTime_RemindsOnlyTheClinicsLocalDay()
        {
            // Arrange - 08:00 at the clinic, which is UTC+2 in September, so tomorrow at the
            // clinic runs from 16 Sep 22:00 UTC to 17 Sep 22:00 UTC.
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 9, 16, 6, 0, 0, TimeSpan.Zero));

            var beforeWindow = AppointmentStartingAt(new DateTime(2026, 9, 16, 21, 30, 0, DateTimeKind.Utc));
            var insideWindow = AppointmentStartingAt(new DateTime(2026, 9, 16, 22, 30, 0, DateTimeKind.Utc));
            var afterWindow = AppointmentStartingAt(new DateTime(2026, 9, 17, 22, 30, 0, DateTimeKind.Utc));

            Seed(beforeWindow, insideWindow, afterWindow);

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert - only the appointment that falls on the clinic's tomorrow is reminded.
            result.IsSuccess.Should().BeTrue();

            await _notificationService.ReceivedWithAnyArgs(1).SendAppointmentReminder(default!, default);
            await _notificationService.Received(1).SendAppointmentReminder(
                Arg.Is<AppointmentReminderDTO>(dto => dto.Id == insideWindow.Id),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_InWinterTime_RemindsOnlyTheClinicsLocalDay()
        {
            // Arrange - 08:00 at the clinic, which is UTC+1 in December, so tomorrow at the
            // clinic runs from 16 Dec 23:00 UTC to 17 Dec 23:00 UTC. The 22:30 UTC appointment
            // would have been inside the summer window and is outside this one.
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 12, 16, 7, 0, 0, TimeSpan.Zero));

            var beforeWindow = AppointmentStartingAt(new DateTime(2026, 12, 16, 22, 30, 0, DateTimeKind.Utc));
            var insideWindow = AppointmentStartingAt(new DateTime(2026, 12, 16, 23, 30, 0, DateTimeKind.Utc));

            Seed(beforeWindow, insideWindow);

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            await _notificationService.ReceivedWithAnyArgs(1).SendAppointmentReminder(default!, default);
            await _notificationService.Received(1).SendAppointmentReminder(
                Arg.Is<AppointmentReminderDTO>(dto => dto.Id == insideWindow.Id),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_TwoAppointmentsTomorrow_SendsOneReminderEach()
        {
            // Arrange
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 9, 16, 6, 0, 0, TimeSpan.Zero));

            var first = AppointmentStartingAt(new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc));
            var second = AppointmentStartingAt(new DateTime(2026, 9, 17, 9, 0, 0, DateTimeKind.Utc));

            Seed(first, second);

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert - each appointment produces one email, carrying the joined names.
            result.IsSuccess.Should().BeTrue();

            await _notificationService.ReceivedWithAnyArgs(2).SendAppointmentReminder(default!, default);

            await _notificationService.Received(1).SendAppointmentReminder(
                Arg.Is<AppointmentReminderDTO>(dto => dto.Id == first.Id
                                                   && dto.Date == first.TimeInterval.Start
                                                   && dto.Patient == "Ana"
                                                   && dto.PatientEmail == "ana@example.com"
                                                   && dto.Dentist == "Claudia"
                                                   && dto.Office == "Main Office"),
                Arg.Any<CancellationToken>());

            await _notificationService.Received(1).SendAppointmentReminder(
                Arg.Is<AppointmentReminderDTO>(dto => dto.Id == second.Id),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_AppointmentTomorrowIsCancelled_SendsNoReminder()
        {
            // Arrange
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 9, 16, 6, 0, 0, TimeSpan.Zero));

            var cancelled = AppointmentStartingAt(new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc));
            cancelled.Cancel(_timeProvider.GetUtcNow().UtcDateTime).IsSuccess.Should().BeTrue();

            Seed(cancelled);

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _notificationService.DidNotReceiveWithAnyArgs()
                .SendAppointmentReminder(default!, default);
        }

        [Fact]
        public async Task Handle_NoAppointmentsTomorrow_SendsNoReminders()
        {
            // Arrange
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 9, 16, 6, 0, 0, TimeSpan.Zero));
            Seed();

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _notificationService.DidNotReceiveWithAnyArgs()
                .SendAppointmentReminder(default!, default);
        }

        // The handler projects straight from the DbSets, so the seam is the context, not a
        // repository: every table the join touches has to be populated.
        private void Seed(params Appointment[] appointments)
        {
            // Each DbSet is built before its Returns call: BuildMockDbSet creates a substitute
            // of its own, and NSubstitute cannot have one configured inside another's Returns.
            var appointmentSet = appointments.ToList().BuildMockDbSet();
            var patientSet = new List<Patient> { _patient }.BuildMockDbSet();
            var dentistSet = new List<Dentist> { _dentist }.BuildMockDbSet();
            var officeSet = new List<Office> { _office }.BuildMockDbSet();

            _db.Appointments.Returns(appointmentSet);
            _db.Patients.Returns(patientSet);
            _db.Dentists.Returns(dentistSet);
            _db.Offices.Returns(officeSet);
        }

        // "Now" for the factory is well before the start, so its not-in-the-past rule never
        // interferes with the instant the test is really interested in.
        private Appointment AppointmentStartingAt(DateTime startUtc) =>
            Appointment.Create(_patient.Id, _dentist.Id, _office.Id,
                               startUtc, startUtc.AddHours(1), startUtc.AddDays(-30)).Value;

        private SendAppointmentRemindersHandler CreateHandler() =>
            new(_db, _notificationService, _timeProvider,
                Options.Create(new ClinicOptions { TimeZoneId = "Europe/Madrid" }),
                Substitute.For<ILogger<SendAppointmentRemindersHandler>>());
    }
}
