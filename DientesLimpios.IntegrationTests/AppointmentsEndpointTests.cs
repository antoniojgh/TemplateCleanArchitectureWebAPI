using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DientesLimpios.API.DTOs.Appointments;
using DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Enums;
using DientesLimpios.Domain.Events;
using DientesLimpios.Persistence;
using DientesLimpios.Persistence.Outbox;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace DientesLimpios.IntegrationTests
{
    [Collection(IntegrationCollection.Name)]
    public sealed class AppointmentsEndpointTests(IntegrationTestFactory factory)
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task Post_ValidAppointment_Returns201_AndAppointmentIsRetrievable()
        {
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();

            var start = DateTime.UtcNow.AddDays(1);

            var appointment = new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            };

            var post = await _client.PostAsJsonAsync("/api/v1/appointments", appointment);

            post.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdId = await post.Content.ReadFromJsonAsync<Guid>();
            createdId.Should().NotBeEmpty();

            var get = await _client.GetAsync(new Uri($"/api/v1/appointments/{createdId}", UriKind.Relative));

            get.StatusCode.Should().Be(HttpStatusCode.OK);
            var detail = await get.Content.ReadFromJsonAsync<AppointmentDetailDTO>();
            detail.Should().NotBeNull();
            // Adjust the property name if your DTO differs:
            detail!.Id.Should().Be(createdId);

            // The event is stored in the outbox and delivered after the request completes.
            // Hosted services are removed in tests, so deliver it explicitly.
            await ProcessOutboxAsync();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.Confirmations.Should().ContainSingle(c => c.Id == createdId);
        }

        [Fact]
        public async Task Post_ValidAppointment_WritesOutboxMessageWithTheAppointment()
        {
            // Arrange
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = DateTime.UtcNow.AddDays(1);

            // Act
            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            });

            // Assert
            post.StatusCode.Should().Be(HttpStatusCode.Created);
            var appointmentId = await post.Content.ReadFromJsonAsync<Guid>();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var messages = await db.OutboxMessages
                .Where(m => m.Type == nameof(AppointmentCreatedEvent) && m.ProcessedOnUtc == null)
                .ToListAsync();

            messages.Select(OutboxSerializer.ToDomainEvent)
                .OfType<AppointmentCreatedEvent>()
                .Should().ContainSingle(e => e.AppointmentId == appointmentId);
        }


        [Fact]
        public async Task Post_OverlappingForSameDentist_Returns409_ProblemDetails()
        {
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();

            var start = DateTime.UtcNow.AddDays(1);

            var appointmentFirst = new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            };

            (await _client.PostAsJsonAsync("/api/v1/appointments", appointmentFirst)).StatusCode.Should().Be(HttpStatusCode.Created);

            // Same dentist, overlapping window.
            var appointmentOverlapping = new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start.AddMinutes(30),
                EndDate = start.AddHours(1).AddMinutes(30)
            };

            var conflict = await _client.PostAsJsonAsync("/api/v1/appointments", appointmentOverlapping);

            conflict.StatusCode.Should().Be(HttpStatusCode.Conflict);          // 409, not 400
            var problem = await conflict.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(409);
            problem.Extensions.Should().ContainKey("errorCode");               // "Appointment.Overlapping"
        }


        [Fact]
        public async Task Post_TwoConcurrentOverlappingRequests_CreatesExactlyOneAppointment()
        {
            // Arrange — same dentist, overlapping windows, fired at the same time.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();

            var start = DateTime.UtcNow.AddDays(1);

            var first = new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            };

            var second = new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start.AddMinutes(30),
                EndDate = start.AddHours(1).AddMinutes(30)
            };

            // Act
            var responses = await Task.WhenAll(
                _client.PostAsJsonAsync("/api/v1/appointments", first),
                _client.PostAsJsonAsync("/api/v1/appointments", second));

            // Assert — the loser must be rejected by the overlap rule, not by a 500.
            responses.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
            responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(1);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            var stored = await db.Appointments.CountAsync(x => x.DentistId == dentistId);
            stored.Should().Be(1);
        }

        [Fact]
        public async Task Delete_DentistWithAppointments_Returns409_AndAppointmentSurvives()
        {
            // Arrange — a dentist with one appointment.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();

            var start = DateTime.UtcNow.AddDays(1);

            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            });

            post.StatusCode.Should().Be(HttpStatusCode.Created);
            var appointmentId = await post.Content.ReadFromJsonAsync<Guid>();

            // Act
            var delete = await _client.DeleteAsync(new Uri($"/api/v1/dentists/{dentistId}", UriKind.Relative));

            // Assert — the delete is refused and the history is intact.
            delete.StatusCode.Should().Be(HttpStatusCode.Conflict);

            var problem = await delete.Content.ReadFromJsonAsync<ProblemDetails>();
            problem!.Extensions.Should().ContainKey("errorCode");

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            (await db.Appointments.AnyAsync(x => x.Id == appointmentId)).Should().BeTrue();
        }

        [Fact]
        public async Task Post_UnknownPatient_Returns404_WithPatientNotFoundCode()
        {
            // Arrange — a real dentist and office, but a patient id that was never stored.
            var (_, dentistId, officeId) = await SeedCoreEntitiesAsync();

            var start = DateTime.UtcNow.AddDays(1);

            var appointment = new CreateAppointmentDTO
            {
                PatientId = Guid.CreateVersion7(),
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/appointments", appointment);

            // Assert — a client error, not the 500 the raw foreign-key violation used to produce.
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(404);
            ErrorCode(problem).Should().Be("Patient.NotFound");
            problem.Detail.Should().NotContain("FOREIGN KEY");

            (await CountAppointmentsAsync(x => x.DentistId == dentistId)).Should().Be(0);
        }

        [Fact]
        public async Task Post_UnknownDentist_Returns404_WithDentistNotFoundCode()
        {
            // Arrange — a real patient and office, but a dentist id that was never stored.
            var (patientId, _, officeId) = await SeedCoreEntitiesAsync();

            var start = DateTime.UtcNow.AddDays(1);

            var appointment = new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = Guid.CreateVersion7(),
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/appointments", appointment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(404);
            ErrorCode(problem).Should().Be("Dentist.NotFound");
            problem.Detail.Should().NotContain("FOREIGN KEY");

            (await CountAppointmentsAsync(x => x.PatientId == patientId)).Should().Be(0);
        }

        [Fact]
        public async Task Post_UnknownOffice_Returns404_WithOfficeNotFoundCode()
        {
            // Arrange — a real patient and dentist, but an office id that was never stored.
            var (patientId, dentistId, _) = await SeedCoreEntitiesAsync();

            var start = DateTime.UtcNow.AddDays(1);

            var appointment = new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = Guid.CreateVersion7(),
                StartDate = start,
                EndDate = start.AddHours(1)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/appointments", appointment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(404);
            ErrorCode(problem).Should().Be("Office.NotFound");
            problem.Detail.Should().NotContain("FOREIGN KEY");

            (await CountAppointmentsAsync(x => x.DentistId == dentistId)).Should().Be(0);
        }

        [Fact]
        public async Task Outbox_ConfirmationFailsOnce_MessageIsRetriedAndEmailSentOnce()
        {
            // Arrange — drain what earlier tests left behind, so the appointment created here
            // is the only pending message.
            await ProcessOutboxAsync();

            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = DateTime.UtcNow.AddDays(1);

            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            });

            post.StatusCode.Should().Be(HttpStatusCode.Created);
            var appointmentId = await post.Content.ReadFromJsonAsync<Guid>();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.FailNextConfirmationFor(appointmentId);

            // Act — first delivery, which the notification service rejects.
            await ProcessOutboxOnceAsync();

            // Assert — the failure is recorded and the message stays pending for a retry.
            var afterFailure = await GetOutboxMessageAsync(appointmentId);
            afterFailure.ProcessedOnUtc.Should().BeNull();
            afterFailure.AttemptCount.Should().Be(1);
            afterFailure.Error.Should().Contain("Simulated SMTP failure");
            notifications.Confirmations.Should().NotContain(c => c.Id == appointmentId);

            // Act — second delivery, which succeeds.
            await ProcessOutboxOnceAsync();

            // Assert — delivered exactly once in total.
            var afterRetry = await GetOutboxMessageAsync(appointmentId);
            afterRetry.ProcessedOnUtc.Should().NotBeNull();
            afterRetry.Error.Should().BeNull();
            notifications.Confirmations.Should().ContainSingle(c => c.Id == appointmentId);
        }

        [Fact]
        public async Task Outbox_MessageRedelivered_ConfirmationIsNotSentTwice()
        {
            // Arrange — one appointment, delivered once.
            await ProcessOutboxAsync();

            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = DateTime.UtcNow.AddDays(1);

            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            });

            post.StatusCode.Should().Be(HttpStatusCode.Created);
            var appointmentId = await post.Content.ReadFromJsonAsync<Guid>();

            await ProcessOutboxOnceAsync();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.Confirmations.Should().ContainSingle(c => c.Id == appointmentId);

            // Act — simulate the crash window: the message was delivered but never marked,
            // so the processor picks it up again.
            var message = await GetOutboxMessageAsync(appointmentId);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
                await db.OutboxMessages
                    .Where(m => m.Id == message.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.ProcessedOnUtc, (DateTime?)null));
            }

            await ProcessOutboxOnceAsync();

            // Assert — the handler recognised the appointment was already confirmed.
            notifications.Confirmations.Should().ContainSingle(c => c.Id == appointmentId);
        }

        [Fact]
        public async Task Post_StartWithOffset_IsStoredAsTheUtcInstant_AndReturnedWithZ()
        {
            // Arrange — 10:00 at UTC+2 is 08:00 UTC.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            using var body = AppointmentJson(patientId, dentistId, officeId,
                "2030-09-01T10:00:00+02:00", "2030-09-01T11:00:00+02:00");

            // Act
            var post = await _client.PostAsync(new Uri("/api/v1/appointments", UriKind.Relative), body);
            post.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await post.Content.ReadFromJsonAsync<Guid>();

            var detail = await _client.GetFromJsonAsync<JsonElement>(
                new Uri($"/api/v1/appointments/{id}", UriKind.Relative));

            // Assert — the same instant, and marked as UTC.
            detail.GetProperty("startDate").GetString().Should().Be("2030-09-01T08:00:00Z");
            detail.GetProperty("endDate").GetString().Should().Be("2030-09-01T09:00:00Z");
        }

        [Fact]
        public async Task Post_StartWithoutOffset_Returns400()
        {
            // Arrange — "10:00" with no zone is ambiguous; the API must refuse to guess.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            using var body = AppointmentJson(patientId, dentistId, officeId,
                "2030-09-01T10:00:00", "2030-09-01T11:00:00");

            // Act
            var post = await _client.PostAsync(new Uri("/api/v1/appointments", UriKind.Relative), body);

            // Assert
            post.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Outbox_AppointmentChangedWhileSending_RecordsTheSendAndDoesNotResend()
        {
            // Arrange — drain earlier messages, then book an appointment.
            await ProcessOutboxAsync();

            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = DateTime.UtcNow.AddDays(1);

            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            });

            post.StatusCode.Should().Be(HttpStatusCode.Created);
            var appointmentId = await post.Content.ReadFromJsonAsync<Guid>();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();

            // Someone cancels the appointment while the confirmation is being sent, which
            // changes the row version the handler loaded.
            notifications.BeforeNextConfirmation(async _ =>
            {
                using var scope = factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

                var appointment = await db.Appointments.FirstAsync(a => a.Id == appointmentId);
                appointment.Cancel(DateTime.UtcNow).IsSuccess.Should().BeTrue();
                await db.SaveChangesAsync();
            });

            // Act
            await ProcessOutboxOnceAsync();

            // Assert — the send is recorded despite the concurrent change...
            (await GetOutboxMessageAsync(appointmentId)).ProcessedOnUtc.Should().NotBeNull();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
                var stored = await db.Appointments.FirstAsync(a => a.Id == appointmentId);

                stored.ConfirmationSentAtUtc.Should().NotBeNull();
                stored.Status.Should().Be(AppointmentStatus.Cancelled);   // the other writer's change survived
            }

            // ...and no retry ever sends a second email.
            await ProcessOutboxOnceAsync();
            notifications.Confirmations.Should().ContainSingle(c => c.Id == appointmentId);
        }

        [Fact]
        public async Task Outbox_CancellationFailsOnce_MessageIsRetriedAndEmailSentOnce()
        {
            // Arrange — a booked appointment whose confirmation is already delivered, so the
            // cancellation is the only pending message.
            await ProcessOutboxAsync();

            var appointmentId = await CreateAppointmentAsync();
            await ProcessOutboxAsync();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.FailNextCancellationFor(appointmentId);

            await CancelAppointmentAsync(appointmentId);

            // Act — first delivery, which the notification service rejects.
            await ProcessOutboxOnceAsync();

            // Assert — the failure is recorded and the message stays pending for a retry.
            var afterFailure = await GetCancellationOutboxMessageAsync(appointmentId);
            afterFailure.ProcessedOnUtc.Should().BeNull();
            afterFailure.AttemptCount.Should().Be(1);
            afterFailure.Error.Should().Contain("Simulated SMTP failure");
            notifications.CancelledConfirmations.Should().NotContain(c => c.Id == appointmentId);

            // Act — second delivery, which succeeds.
            await ProcessOutboxOnceAsync();

            // Assert — delivered exactly once in total.
            var afterRetry = await GetCancellationOutboxMessageAsync(appointmentId);
            afterRetry.ProcessedOnUtc.Should().NotBeNull();
            afterRetry.Error.Should().BeNull();
            notifications.CancelledConfirmations.Should().ContainSingle(c => c.Id == appointmentId);
        }

        [Fact]
        public async Task Outbox_CancellationRedelivered_EmailIsNotSentTwice()
        {
            // Arrange — a cancelled appointment whose cancellation email was delivered once.
            await ProcessOutboxAsync();

            var appointmentId = await CreateAppointmentAsync();
            await ProcessOutboxAsync();

            await CancelAppointmentAsync(appointmentId);
            await ProcessOutboxOnceAsync();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.CancelledConfirmations.Should().ContainSingle(c => c.Id == appointmentId);

            // Act — simulate the crash window: the message was delivered but never marked,
            // so the processor picks it up again.
            var message = await GetCancellationOutboxMessageAsync(appointmentId);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
                await db.OutboxMessages
                    .Where(m => m.Id == message.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.ProcessedOnUtc, (DateTime?)null));
            }

            await ProcessOutboxOnceAsync();

            // Assert — the handler found the delivery marker and did not send again.
            notifications.CancelledConfirmations.Should().ContainSingle(c => c.Id == appointmentId);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
                var stored = await db.Appointments.FirstAsync(a => a.Id == appointmentId);

                stored.CancellationSentAtUtc.Should().NotBeNull();
            }
        }


        [Fact]
        public async Task Reschedule_ToFreeSlot_Returns200_AndStoresTheNewInterval()
        {
            // Arrange
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));

            var newStart = start.AddDays(1);

            // Act
            var response = await RescheduleAsync(appointmentId, newStart, newStart.AddHours(1));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadFromJsonAsync<Guid>()).Should().Be(appointmentId);

            var stored = await GetStoredAppointmentAsync(appointmentId);
            stored.TimeInterval.Start.Should().Be(newStart);
            stored.TimeInterval.End.Should().Be(newStart.AddHours(1));
            stored.Status.Should().Be(AppointmentStatus.Scheduled);
        }

        [Fact]
        public async Task Reschedule_OntoAnotherAppointmentOfSameDentist_Returns409_AndKeepsTheOriginalSlot()
        {
            // Arrange — two appointments for the same dentist on different days.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var toMove = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            var blocking = start.AddDays(1);
            await BookAsync(patientId, dentistId, officeId, blocking, blocking.AddHours(1));

            // Act — move the first one half an hour into the second.
            var response = await RescheduleAsync(toMove, blocking.AddMinutes(30), blocking.AddMinutes(90));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            ErrorCode(problem!).Should().Be("Appointment.Overlapping");

            (await GetStoredAppointmentAsync(toMove)).TimeInterval.Start.Should().Be(start);
        }

        [Fact]
        public async Task Reschedule_OverlappingItsOwnCurrentSlot_Returns200()
        {
            // Arrange — the dentist has nothing else booked.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));

            // Act — push it back 30 minutes, so the new slot overlaps the one it is leaving.
            var response = await RescheduleAsync(appointmentId, start.AddMinutes(30), start.AddMinutes(90));

            // Assert — an appointment must not block itself.
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await GetStoredAppointmentAsync(appointmentId)).TimeInterval.Start.Should().Be(start.AddMinutes(30));
        }

        [Fact]
        public async Task Reschedule_ReleasesTheOldSlot_SoItCanBeBookedAgain()
        {
            // Arrange
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));

            var newStart = start.AddDays(1);
            (await RescheduleAsync(appointmentId, newStart, newStart.AddHours(1)))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            // Act — book the slot the appointment just left.
            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            });

            // Assert
            post.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Reschedule_OntoACancelledAppointmentsSlot_Returns200()
        {
            // Arrange — a cancelled appointment no longer occupies its slot.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var toMove = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            var freed = start.AddDays(1);
            var cancelled = await BookAsync(patientId, dentistId, officeId, freed, freed.AddHours(1));
            await CancelAppointmentAsync(cancelled);

            // Act
            var response = await RescheduleAsync(toMove, freed, freed.AddHours(1));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Reschedule_CancelledAppointment_Returns400_WithOnlyScheduledCode()
        {
            // Arrange
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            await CancelAppointmentAsync(appointmentId);

            // Act
            var response = await RescheduleAsync(appointmentId, start.AddDays(1), start.AddDays(1).AddHours(1));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            ErrorCode(problem!).Should().Be("Appointment.OnlyScheduledCanBeRescheduled");
            (await GetStoredAppointmentAsync(appointmentId)).TimeInterval.Start.Should().Be(start);
        }

        [Fact]
        public async Task Reschedule_UnknownAppointment_Returns404()
        {
            // Arrange
            var start = WholeHourDaysAhead(2);

            // Act
            var response = await RescheduleAsync(Guid.CreateVersion7(), start, start.AddHours(1));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            ErrorCode(problem!).Should().Be("Appointment.NotFound");
        }

        [Fact]
        public async Task Reschedule_StartInThePast_Returns400_AndKeepsTheOriginalSlot()
        {
            // Arrange
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));

            var past = WholeHourDaysAhead(-1);

            // Act
            var response = await RescheduleAsync(appointmentId, past, past.AddHours(1));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await GetStoredAppointmentAsync(appointmentId)).TimeInterval.Start.Should().Be(start);
        }


        [Fact]
        public async Task Outbox_Rescheduled_SendsEmailWithTheNewTimes()
        {
            // Arrange — a booked appointment whose confirmation is already delivered.
            await ProcessOutboxAsync();

            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            await ProcessOutboxAsync();

            var newStart = start.AddDays(1);
            (await RescheduleAsync(appointmentId, newStart, newStart.AddHours(1)))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            await ProcessOutboxAsync();

            // Assert — one email, carrying the new slot rather than the old one.
            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            var email = notifications.RescheduledConfirmations.Should()
                .ContainSingle(c => c.Id == appointmentId).Subject;

            email.NewStartDate.Should().Be(newStart);
            email.NewEndDate.Should().Be(newStart.AddHours(1));
            email.PatientEmail.Should().Be("patient@test.com");
        }

        [Fact]
        public async Task Outbox_RescheduledEmailFailsOnce_MessageIsRetriedAndEmailSentOnce()
        {
            // Arrange — the reschedule is the only pending message.
            await ProcessOutboxAsync();

            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            await ProcessOutboxAsync();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.FailNextRescheduledFor(appointmentId);

            var newStart = start.AddDays(1);
            (await RescheduleAsync(appointmentId, newStart, newStart.AddHours(1)))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            // Act — first delivery, which the notification service rejects.
            await ProcessOutboxOnceAsync();

            // Assert — the failure is recorded and the message stays pending for a retry.
            var afterFailure = await GetRescheduledOutboxMessageAsync(appointmentId);
            afterFailure.ProcessedOnUtc.Should().BeNull();
            afterFailure.AttemptCount.Should().Be(1);
            afterFailure.Error.Should().Contain("Simulated SMTP failure");
            notifications.RescheduledConfirmations.Should().NotContain(c => c.Id == appointmentId);

            // Act — second delivery, which succeeds.
            await ProcessOutboxOnceAsync();

            // Assert — delivered exactly once in total.
            var afterRetry = await GetRescheduledOutboxMessageAsync(appointmentId);
            afterRetry.ProcessedOnUtc.Should().NotBeNull();
            afterRetry.Error.Should().BeNull();
            notifications.RescheduledConfirmations.Should().ContainSingle(c => c.Id == appointmentId);
        }

        [Fact]
        public async Task Outbox_RescheduledRedelivered_EmailIsNotSentTwice()
        {
            // Arrange — a rescheduled appointment whose email was delivered once.
            await ProcessOutboxAsync();

            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            await ProcessOutboxAsync();

            var newStart = start.AddDays(1);
            (await RescheduleAsync(appointmentId, newStart, newStart.AddHours(1)))
                .StatusCode.Should().Be(HttpStatusCode.OK);
            await ProcessOutboxOnceAsync();

            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.RescheduledConfirmations.Should().ContainSingle(c => c.Id == appointmentId);

            // Act — simulate the crash window: the message was delivered but never marked,
            // so the processor picks it up again.
            var message = await GetRescheduledOutboxMessageAsync(appointmentId);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
                await db.OutboxMessages
                    .Where(m => m.Id == message.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.ProcessedOnUtc, (DateTime?)null));
            }

            await ProcessOutboxOnceAsync();

            // Assert — the outbox delivers at least once, so the handler must recognise the repeat.
            notifications.RescheduledConfirmations.Should().ContainSingle(c => c.Id == appointmentId);
        }

        [Fact]
        public async Task Outbox_RescheduledEmail_KeepsTheBookingConfirmationTime()
        {
            // Arrange — booked and confirmed.
            await ProcessOutboxAsync();

            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var appointmentId = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            await ProcessOutboxAsync();

            var confirmedAtUtc = (await GetStoredAppointmentAsync(appointmentId)).ConfirmationSentAtUtc;
            confirmedAtUtc.Should().NotBeNull();

            var newStart = start.AddDays(1);
            (await RescheduleAsync(appointmentId, newStart, newStart.AddHours(1)))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            await ProcessOutboxAsync();

            // Assert — ConfirmationSentAtUtc records the booking email; the reschedule email
            // must not overwrite it.
            (await GetStoredAppointmentAsync(appointmentId)).ConfirmationSentAtUtc.Should().Be(confirmedAtUtc);
        }

        [Fact]
        public async Task Reschedule_Overlapping_WritesNoRescheduledOutboxMessage()
        {
            // Arrange — two appointments for the same dentist on different days.
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = WholeHourDaysAhead(2);
            var toMove = await BookAsync(patientId, dentistId, officeId, start, start.AddHours(1));
            var blocking = start.AddDays(1);
            await BookAsync(patientId, dentistId, officeId, blocking, blocking.AddHours(1));

            // Act
            (await RescheduleAsync(toMove, blocking, blocking.AddHours(1)))
                .StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Assert — a refused reschedule is not a fact, so nothing is queued to announce it.
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var messages = await db.OutboxMessages
                .Where(m => m.Type == nameof(AppointmentRescheduledEvent))
                .ToListAsync();

            messages.Select(OutboxSerializer.ToDomainEvent)
                    .OfType<AppointmentRescheduledEvent>()
                    .Should().NotContain(e => e.AppointmentId == toMove);
        }


        private static StringContent AppointmentJson(Guid patientId, Guid dentistId, Guid officeId,
                                                     string start, string end) =>
            new($$"""
                {
                  "patientId": "{{patientId}}",
                  "dentistId": "{{dentistId}}",
                  "officeId": "{{officeId}}",
                  "startDate": "{{start}}",
                  "endDate": "{{end}}"
                }
                """, Encoding.UTF8, "application/json");


        // ProblemDetails.Extensions values arrive as JsonElement after deserialisation.
        private static string? ErrorCode(ProblemDetails problem)
        {
            if (!problem.Extensions.TryGetValue("errorCode", out var value))
                return null;

            return value is JsonElement element ? element.GetString() : value?.ToString();
        }

        private async Task<int> CountAppointmentsAsync(Expression<Func<Appointment, bool>> predicate)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            return await db.Appointments.CountAsync(predicate);
        }

        private async Task<(Guid patientId, Guid dentistId, Guid officeId)> SeedCoreEntitiesAsync()
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var patient = Patient.Create("Test Patient", "patient@test.com").Value;
            var dentist = Dentist.Create("Test Dentist", "dentist@test.com").Value;
            var office = Office.Create("Main Office").Value;

            db.Patients.Add(patient);
            db.Dentists.Add(dentist);
            db.Offices.Add(office);
            await db.SaveChangesAsync();

            return (patient.Id, dentist.Id, office.Id);
        }

        // Delivers every pending outbox message, the way OutboxProcessorJob would in production.
        // Keeps processing while batches come back full, because other tests in this collection
        // leave their own messages in the shared database.
        private async Task ProcessOutboxAsync()
        {
            using var scope = factory.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

            while (await processor.ProcessBatch(CancellationToken.None) == OutboxProcessor.BatchSize) { }
        }

        // Processes exactly one batch. The tests below assert on attempt counts, so they
        // cannot use ProcessOutboxAsync: its drain loop could retry the same message again
        // within one call.
        private async Task<int> ProcessOutboxOnceAsync()
        {
            using var scope = factory.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

            return await processor.ProcessBatch(CancellationToken.None);
        }

        // Books a fresh appointment for tomorrow through the API and returns its id.
        private async Task<Guid> CreateAppointmentAsync()
        {
            var (patientId, dentistId, officeId) = await SeedCoreEntitiesAsync();
            var start = DateTime.UtcNow.AddDays(1);

            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = start.AddHours(1)
            });

            post.StatusCode.Should().Be(HttpStatusCode.Created);
            return await post.Content.ReadFromJsonAsync<Guid>();
        }

        // A whole hour, so the value survives the JSON and datetime2 round trips unchanged and
        // the reschedule tests can compare instants exactly.
        private static DateTime WholeHourDaysAhead(int days) =>
            DateTime.UtcNow.Date.AddDays(days).AddHours(10);

        private async Task<Guid> BookAsync(Guid patientId, Guid dentistId, Guid officeId,
                                           DateTime start, DateTime end)
        {
            var post = await _client.PostAsJsonAsync("/api/v1/appointments", new CreateAppointmentDTO
            {
                PatientId = patientId,
                DentistId = dentistId,
                OfficeId = officeId,
                StartDate = start,
                EndDate = end
            });

            post.StatusCode.Should().Be(HttpStatusCode.Created);
            return await post.Content.ReadFromJsonAsync<Guid>();
        }

        private Task<HttpResponseMessage> RescheduleAsync(Guid appointmentId, DateTime start, DateTime end) =>
            _client.PostAsJsonAsync("/api/v1/appointments/reschedule", new RescheduleAppointmentDTO
            {
                Id = appointmentId,
                StartDate = start,
                EndDate = end
            });

        // A new scope, so the state comes from the database and not from a DbContext cache.
        private async Task<Appointment> GetStoredAppointmentAsync(Guid appointmentId)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            return await db.Appointments.AsNoTracking().FirstAsync(a => a.Id == appointmentId);
        }

        private async Task CancelAppointmentAsync(Guid appointmentId)
        {
            using var cancel = await _client.PostAsync(
                new Uri($"/api/v1/appointments/cancel/{appointmentId}", UriKind.Relative), content: null);

            cancel.IsSuccessStatusCode.Should().BeTrue();
        }

        // Reads the outbox row for one appointment. A new scope each time, so the state comes
        // from the database and not from a DbContext cache.
        private async Task<OutboxMessage> GetOutboxMessageAsync(Guid appointmentId)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var messages = await db.OutboxMessages
                .Where(m => m.Type == nameof(AppointmentCreatedEvent))
                .ToListAsync();

            return messages.Single(m => OutboxSerializer.ToDomainEvent(m) is AppointmentCreatedEvent e
                                        && e.AppointmentId == appointmentId);
        }

        private async Task<OutboxMessage> GetRescheduledOutboxMessageAsync(Guid appointmentId)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var messages = await db.OutboxMessages
                .Where(m => m.Type == nameof(AppointmentRescheduledEvent))
                .ToListAsync();

            return messages.Single(m => OutboxSerializer.ToDomainEvent(m) is AppointmentRescheduledEvent e
                                        && e.AppointmentId == appointmentId);
        }

        private async Task<OutboxMessage> GetCancellationOutboxMessageAsync(Guid appointmentId)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var messages = await db.OutboxMessages
                .Where(m => m.Type == nameof(AppointmentCancelledEvent))
                .ToListAsync();

            return messages.Single(m => OutboxSerializer.ToDomainEvent(m) is AppointmentCancelledEvent e
                                        && e.AppointmentId == appointmentId);
        }

    }

}
