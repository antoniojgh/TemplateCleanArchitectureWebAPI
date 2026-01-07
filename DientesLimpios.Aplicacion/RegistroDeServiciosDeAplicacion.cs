using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.Behaviors;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio;
using DientesLimpios.Aplicacion.Mappings;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Aplicacion
{
    public static class RegistroDeServiciosDeAplicacion
    {
        public static IServiceCollection AgregarServiciosDeAplicacion(
                    this IServiceCollection services)
        {
            // OR, if you are using the modern MediatR 12+ registration:
            services.AddMediatR(cfg => {
                // 1. Register Handlers
                cfg.RegisterServicesFromAssembly(typeof(ValidationBehavior<,>).Assembly); // Your Application Assembly

                // 2. Register Behaviors (The Modern Way)
                // You no longer need to write "typeof(IPipelineBehavior<,>)" explicitly.
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // Register all FluentValidation validators from the assembly
            services.AddValidatorsFromAssembly(typeof(ValidadorComandoCrearConsultorio).Assembly);

            // "typeof(HandlerCrearConsultorio)" can be ANY class in your Application project
            // It serves as a marker so AutoMapper knows which assembly to scan.
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            return services;

        }
    }
}
