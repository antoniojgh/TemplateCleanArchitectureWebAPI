using System.Net;
using System.Net.Http.Json;
using DientesLimpios.API.DTOs.Appointments;
using DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
