using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DientesLimpios.Persistence.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly DientesLimpiosDbContext context;

        public AppointmentRepository(DientesLimpiosDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<bool> AppointmentOverlaps(Guid dentistId, DateTime start, DateTime end)
        {
            return await context.Appointments
                .Where(x => x.DentistId == dentistId && x.Status == Domain.Enums.AppointmentStatus.Scheduled &&
                start < x.TimeInterval.End && end > x.TimeInterval.Start
                ).AnyAsync();
        }

        public async Task<IEnumerable<Appointment>> GetFiltered(AppointmentFilterDTO appointmentFilterDTO)
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
                .ToListAsync();

        }

        new public async Task<Appointment?> GetById(Guid id)
        {
            return await context.Appointments
                .Include(x => x.Patient)
                .Include(x => x.Dentist)
                .Include(x => x.Office)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}
