using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Persistencia.Repositorios;
using DientesLimpios.Persistencia.UnidadesDeTrabajo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Persistencia
{
    public static class RegistroDeServiciosDePersistencia
    {
        public static IServiceCollection AgregarServiciosDePersistencia(this IServiceCollection services)
        {
            services.AddDbContext<DientesLimpiosDbContext>(options =>
                options.UseSqlServer("name=DientesLimpiosConnectionString"));


            //Inyección de dependencias de los repositorios
            services.AddScoped<IRepositorioConsultorios, RepositorioConsultorios>();
            services.AddScoped<IRepositorioPacientes, RepositorioPacientes>();
            services.AddScoped<IRepositorioDentistas, RepositorioDentistas>();
            services.AddScoped<IRepositorioCitas, RepositorioCitas>();

            services.AddScoped<IUnitOfwork, UnidadDeTrabajoEFCore>();

            return services;
        }
    }
}
