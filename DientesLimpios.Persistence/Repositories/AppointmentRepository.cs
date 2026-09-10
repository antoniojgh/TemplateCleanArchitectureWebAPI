using System.Data;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public sealed class AppointmentRepository(DientesLimpiosDbContext context) : IAppointmentRepository
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
            var appointmentResult = Appointment.Create(patientId, dentistId, officeId, start, end, DateTime.UtcNow);

            if (appointmentResult.IsFailure)
                return Result.Failure<Guid>(appointmentResult.Error);

            context.Appointments.Add(appointmentResult.Value);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(appointmentResult.Value.Id);
        }

        public async Task<IEnumerable<Appointment>> GetFiltered(AppointmentFilterDTO appointmentFilterDTO, CancellationToken cancellationToken = default)
        {
            var queryable = context.Appointments
                                .Include(x => x.Patient)
                                .Include(x => x.Dentist)
                                .Include(x => x.Office)
                                .AsQueryable();

            if (appointmentFilterDTO.OfficeId is not null)
            {
                queryable = queryable.Where(x => x.OfficeId == appointmentFilterDTO.OfficeId);
            }

            if (appointmentFilterDTO.DentistId is not null)
            {
                queryable = queryable.Where(x => x.DentistId == appointmentFilterDTO.DentistId);
            }

            if (appointmentFilterDTO.PatientId is not null)
            {
                queryable = queryable.Where(x => x.PatientId == appointmentFilterDTO.PatientId);
            }

            if (appointmentFilterDTO.AppointmentStatus is not null)
            {
                queryable = queryable.Where(x => x.Status == appointmentFilterDTO.AppointmentStatus);
            }

            if (appointmentFilterDTO.StartDate.HasValue)
            {
                queryable = queryable.Where(x => x.TimeInterval.Start >= appointmentFilterDTO.StartDate.Value);
            }

            if (appointmentFilterDTO.EndDate.HasValue)
            {
                queryable = queryable.Where(x => x.TimeInterval.End < appointmentFilterDTO.EndDate.Value);
            }

            return await queryable
                .OrderBy(x => x.TimeInterval.Start)
                .ToListAsync(cancellationToken);

        }

        public async Task<Appointment?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.Appointments
                .Include(x => x.Patient)
                .Include(x => x.Dentist)
                .Include(x => x.Office)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

    }
}
