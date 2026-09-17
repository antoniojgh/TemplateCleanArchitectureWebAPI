using DientesLimpios.API.Jobs;
using DientesLimpios.Application.Configuration;
using DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using FluentAssertions;

namespace DientesLimpios.Tests.API.Jobs
{
    public class AppointmentReminderJobTests
    {
        private readonly IMediator _mediator = Substitute.For<IMediator>();

        [Fact]
        public async Task StartAsync_BeforeEightAtTheClinic_SendsRemindersOnceItIsEight()
        {
            // Arrange — 07:30 at the clinic (UTC+2 in September).
            var clock = new FakeTimeProvider(new DateTimeOffset(2026, 9, 16, 5, 30, 0, TimeSpan.Zero));
            var sent = SignalWhenRemindersAreSent();

            await using var services = BuildServices();
            using var job = CreateJob(services, clock);

            // Act — the first check sees 07:30, so the job starts waiting an hour.
            await job.StartAsync(CancellationToken.None);

            // Assert — nothing yet.
            sent.Task.IsCompleted.Should().BeFalse();

            // Act — an hour passes; it is now 08:30 at the clinic.
            clock.Advance(TimeSpan.FromHours(1));

            // Assert
            await sent.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await _mediator.Received(1).Send(
                Arg.Is<IRequest<Result>>(r => r is SendAppointmentRemindersCommand),
                Arg.Any<CancellationToken>());

            await job.StopAsync(CancellationToken.None);
        }

        [Fact]
    public async Task StartAsync_AtEightInWinter_SendsRemindersImmediately()
    {
        // Arrange — 07:15 UTC in December is 08:15 at the clinic (UTC+1).
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 12, 16, 7, 15, 0, TimeSpan.Zero));
        var sent = SignalWhenRemindersAreSent();

        await using var services = BuildServices();
        using var job = CreateJob(services, clock);

        // Act
        await job.StartAsync(CancellationToken.None);

        // Assert — no time has to pass.
        await sent.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await job.StopAsync(CancellationToken.None);
    }

    // Completes when the job sends the command. The job runs on its own task, so the test
    // awaits this signal instead of asserting immediately after Advance.
    private TaskCompletionSource SignalWhenRemindersAreSent()
    {
        var sent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
                 .Returns(_ =>
                 {
                     sent.TrySetResult();
                     return Task.FromResult(Result.Success());
                 });

        return sent;
    }

    // The job creates a DI scope per run, so give it a real container holding the substitute.
    private ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_mediator);
        return services.BuildServiceProvider();
    }

    private static AppointmentReminderJob CreateJob(ServiceProvider services, TimeProvider clock) =>
        new(services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new ClinicOptions { TimeZoneId = "Europe/Madrid" }),
            clock,
            NullLogger<AppointmentReminderJob>.Instance);
}
}
