using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Persistence.Repositories;
using DientesLimpios.Persistence.UnitsOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AgregarServicesDePersistence(this IServiceCollection services)
        {
            services.AddDbContext<DientesLimpiosDbContext>(options =>
                options.UseSqlServer("name=DientesLimpiosConnectionString"));


            //Inyección de dependencias de los repositorios
            services.AddScoped<IOfficeRepository, OfficeRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IDentistRepository, DentistRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            services.AddScoped<IUnitOfWork, EFCoreUnitOfWork>();

            return services;
        }
    }
}
