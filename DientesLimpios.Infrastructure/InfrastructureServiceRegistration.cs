using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AgregarServicesDeInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<EmailOptions>()
                .Bind(configuration.GetSection(EmailOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddScoped<INotificationService, EmailService>();
            return services;
        }
    }
}
