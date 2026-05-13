using DientesLimpios.Application.Utilities.Mediator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AgregarServicesDeApplication(
                    this IServiceCollection services)
        {
            services.AddTransient<IMediator, SimpleMediator>();

            // Registers all AbstractValidator<T> from the assembly
            services.AddValidatorsFromAssembly(typeof(IMediator).Assembly);

            services.Scan(scan =>
            scan.FromAssembliesOf(typeof(IMediator))
            .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

            return services;

        }
    }
}
