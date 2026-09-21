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
                appointment.Cancel().IsSuccess.Should().BeTrue();
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

    }

}
