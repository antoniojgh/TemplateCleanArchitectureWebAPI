using System.Data;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public sealed class AppointmentRepository(DientesLimpiosDbContext context, TimeProvider timeProvider) : IAppointmentRepository
    {
        // How long a caller waits for another booking on the same dentist to finish
        // before the request fails outright rather than queueing indefinitely.
        private const int LockTimeoutMilliseconds = 5000;

        public async Task<Result<Guid>> AddIfNoOverlap(Guid patientId, Guid dentistId, Guid officeId, DateTime start,DateTime end, CancellationToken cancellationToken = default)
        {
            var lockResource = $"appointment:dentist:{dentistId}";

            await using var transaction = await context.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            // "These two intervals do not overlap" cannot be expressed as a unique index, so the
            // invariant is protected with an application lock instead. Keying it on the dentist
            // means concurrent bookings for different dentists never block each other, and taking
            // it before the read means there is no shared-to-exclusive upgrade to deadlock on.
            // sp_getapplock reports failure through its return value, not through an error.
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"""
         DECLARE @lockResult int;
         EXEC @lockResult = sp_getapplock
              @Resource = {lockResource},
              @LockMode = 'Exclusive',
              @LockOwner = 'Transaction',
              @LockTimeout = {LockTimeoutMilliseconds};
         IF @lockResult < 0
             THROW 51000, 'Could not acquire the dentist booking lock.', 1;
         """,
                cancellationToken);

            var overlaps = await context.Appointments
                .AnyAsync(x => x.DentistId == dentistId &&
                               x.Status == Domain.Enums.AppointmentStatus.Scheduled &&
                               start < x.TimeInterval.End &&
                               end > x.TimeInterval.Start,
                          cancellationToken);

            if (overlaps)
                return Result.Failure<Guid>(DomainErrors.Appointment.Overlapping);

            // The slot is free and the lock is held until this transaction ends, so the booking
            // is now a fact. Only here is it correct to construct the aggregate and let it raise
            // its creation event.
            var appointmentResult = Appointment.Create(patientId, dentistId, officeId, start, end, timeProvider.GetUtcNow().UtcDateTime);

            if (appointmentResult.IsFailure)
                return Result.Failure<Guid>(appointmentResult.Error);

            context.Appointments.Add(appointmentResult.Value);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(appointmentResult.Value.Id);
        }
        public async Task<Appointment?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.Appointments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

    }
}
