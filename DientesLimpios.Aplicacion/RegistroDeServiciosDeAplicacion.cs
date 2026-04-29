using DientesLimpios.Aplicacion.Utilidades.Mediador;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Aplicacion
{
    public static class RegistroDeServiciosDeAplicacion
    {
        public static IServiceCollection AgregarServiciosDeAplicacion(
                    this IServiceCollection services)
        {
            services.AddTransient<IMediator, MediadorSimple>();

            // Registers all AbstractValidator<T> from the assembly
            services.AddValidatorsFromAssembly(typeof(IMediator).Assembly);

            services.Scan(scan =>
            scan.FromAssembliesOf(typeof(IMediator))
            .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

            return services;

        }
    }
}
