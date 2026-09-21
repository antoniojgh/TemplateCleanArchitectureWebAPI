using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Enums;
using DientesLimpios.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.IntegrationTests
{
    [Collection(IntegrationCollection.Name)]
    public sealed class ConcurrencyTests(IntegrationTestFactory factory)
    {
        [Fact]
        public async Task SaveChanges_AppointmentAlreadyChangedByAnotherScope_ThrowsConcurrencyException()
        {
            // Arrange — the same appointment loaded twice, before either write.
            var appointmentId = await SeedScheduledAppointmentAsync();

            using var scopeA = factory.Services.CreateScope();
            using var scopeB = factory.Services.CreateScope();
            var dbA = scopeA.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            var dbB = scopeB.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var inScopeA = await dbA.Appointments.FirstAsync(x => x.Id == appointmentId);
            var inScopeB = await dbB.Appointments.FirstAsync(x => x.Id == appointmentId);

            // Act — A cancels and commits. B still holds the row as it was, so its
            // in-memory Scheduled check passes and it tries to complete the same appointment.
            inScopeA.Cancel().IsSuccess.Should().BeTrue();
            await dbA.SaveChangesAsync();

            inScopeB.Complete().IsSuccess.Should().BeTrue();
            var act = () => dbB.SaveChangesAsync();

            // Assert — the second write is refused, and the first writer's state survives.
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();

            using var scopeC = factory.Services.CreateScope();
            var db = scopeC.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            (await db.Appointments.FirstAsync(x => x.Id == appointmentId))
                .Status.Should().Be(AppointmentStatus.Cancelled);
        }

        private async Task<Guid> SeedScheduledAppointmentAsync()
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();

            var patient = Patient.Create("Concurrency Patient", "concurrency@test.com").Value;
            var dentist = Dentist.Create("Concurrency Dentist", "concurrency.dentist@test.com").Value;
            var office = Office.Create("Concurrency Office").Value;

            db.Patients.Add(patient);
            db.Dentists.Add(dentist);
            db.Offices.Add(office);

            var start = DateTime.UtcNow.AddDays(3);
            var appointment = Appointment.Create(patient.Id, dentist.Id, office.Id,
                start, start.AddHours(1), DateTime.UtcNow).Value;

            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();

            return appointment.Id;
        }
    }
}