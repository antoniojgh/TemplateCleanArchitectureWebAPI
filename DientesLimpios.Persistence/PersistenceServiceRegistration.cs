using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Persistence.Interceptors;
using DientesLimpios.Persistence.Outbox;
using DientesLimpios.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AgregarServicesDePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            
            var connectionString = configuration.GetConnectionString("DientesLimpiosConnectionString")
                ?? throw new InvalidOperationException("Connection string 'DientesLimpiosConnectionString' is not configured.");

            // Interceptors
            services.AddScoped<AuditableEntitiesInterceptor>();
            services.AddSingleton<InsertOutboxMessagesInterceptor>();


            // CHANGED: AddDbContext now receives the IServiceProvider so it can resolve the interceptor.
            services.AddDbContext<DientesLimpiosDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
                options.AddInterceptors(
                    sp.GetRequiredService<AuditableEntitiesInterceptor>(),
                    sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
            });


            // NEW — forward IApplicationDbContext to the same DbContext instance:
            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<DientesLimpiosDbContext>());

            //Dependency injection for repositories
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IDentistRepository, DentistRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            // Dependency injection for OutboxProcessor
            services.AddScoped<OutboxProcessor>();

            return services;
        }
    }
}
