using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Persistence.Repositories;
using DientesLimpios.Persistence.UnitsOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DientesLimpios.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AgregarServicesDePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            
            var connectionString = configuration.GetConnectionString("DientesLimpiosConnectionString")
                ?? throw new InvalidOperationException("Connection string 'DientesLimpiosConnectionString' is not configured.");

            services.AddDbContext<DientesLimpiosDbContext>(options =>
                    options.UseSqlServer(connectionString));

            // NEW — forward IApplicationDbContext to the same DbContext instance:
            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<DientesLimpiosDbContext>());


            //Dependency injection for repositories
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IDentistRepository, DentistRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            services.AddScoped<IUnitOfWork, EFCoreUnitOfWork>();

            return services;
        }
    }
}
