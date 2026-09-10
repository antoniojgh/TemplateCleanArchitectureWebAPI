using System.Net;
using System.Net.Http.Json;
using DientesLimpios.API.DTOs.Appointments;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail;


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

            // The domain event is now dispatched after the transaction commits, not inside
            // it. Assert it still reaches its handler — deferring must not mean dropping.
            var notifications = factory.Services.GetRequiredService<RecordingNotificationService>();
            notifications.Confirmations.Should().ContainSingle(c => c.Id == createdId);
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
    }

}
