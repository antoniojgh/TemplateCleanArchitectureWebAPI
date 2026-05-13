using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Infrastructure.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AgregarServicesDeInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<INotificationService, EmailService>();
            return services;
        }
    }
}
