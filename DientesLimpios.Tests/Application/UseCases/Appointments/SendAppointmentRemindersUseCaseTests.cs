using DientesLimpios.Application.Configuration;
using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Appointments
{
    public class SendAppointmentRemindersUseCaseTests
    {
        private readonly IAppointmentRepository _repository = Substitute.For<IAppointmentRepository>();
        private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
        private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

        [Fact]
        public async Task Handle_InSummerTime_QueriesTheClinicsLocalDayInUtc()
        {
            // Arrange - 08:00 at the clinic, which is UTC+2 in September.
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 9, 16, 6, 0, 0, TimeSpan.Zero));
            ReturnsNoAppointments();

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert - local 17 Sep 00:00 is 16 Sep 22:00 UTC.
            result.IsSuccess.Should().BeTrue();

            await _repository.Received(1).GetFiltered(
                Arg.Is<AppointmentFilterDTO>(f => f.StartDate == new DateTime(2026, 9, 16, 22, 0, 0, DateTimeKind.Utc)
                                               && f.EndDate == new DateTime(2026, 9, 17, 22, 0, 0, DateTimeKind.Utc)
                                               && f.AppointmentStatus == AppointmentStatus.Scheduled),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_InWinterTime_QueriesTheClinicsLocalDayInUtc()
        {
            // Arrange - 08:00 at the clinic, which is UTC+1 in December.
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 12, 16, 7, 0, 0, TimeSpan.Zero));
            ReturnsNoAppointments();

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert - local 17 Dec 00:00 is 16 Dec 23:00 UTC.
            result.IsSuccess.Should().BeTrue();

            await _repository.Received(1).GetFiltered(
                Arg.Is<AppointmentFilterDTO>(f => f.StartDate == new DateTime(2026, 12, 16, 23, 0, 0, DateTimeKind.Utc)
                                               && f.EndDate == new DateTime(2026, 12, 17, 23, 0, 0, DateTimeKind.Utc)),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_NoAppointmentsTomorrow_SendsNoReminders()
        {
            // Arrange
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 9, 16, 6, 0, 0, TimeSpan.Zero));
            ReturnsNoAppointments();

            // Act
            var result = await CreateHandler().Handle(new SendAppointmentRemindersCommand(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _notificationService.DidNotReceiveWithAnyArgs()
                .SendAppointmentReminder(default!, default);
        }

        private void ReturnsNoAppointments()
        {
            IEnumerable<Appointment> none = [];

            _repository.GetFiltered(Arg.Any<AppointmentFilterDTO>(), Arg.Any<CancellationToken>())
                       .Returns(none);
        }

        private SendAppointmentRemindersHandler CreateHandler() =>
            new(_repository, _notificationService, _timeProvider,
                Options.Create(new ClinicOptions { TimeZoneId = "Europe/Madrid" }),
                Substitute.For<ILogger<SendAppointmentRemindersHandler>>());
    }
}
