using DientesLimpios.Application.Interfaces.Identity;
using DientesLimpios.Identity.Models;
using DientesLimpios.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DientesLimpios.Identity
{
    public static class IdentityServiceRegistration
    {
        public static IServiceCollection AgregarServicesDeIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DientesLimpiosConnectionString")
                                   ?? throw new InvalidOperationException("Connection string 'DientesLimpiosConnectionString' is not configured.");

            services.AddAuthentication(IdentityConstants.BearerScheme).AddBearerToken(IdentityConstants.BearerScheme);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("esadmin", policy => policy.RequireClaim("esadmin"));
            });

            services.AddDbContext<DientesLimpiosIdentityDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<DientesLimpiosIdentityDbContext>()
                .AddApiEndpoints();

            services.AddTransient<IUserService, UserService>();
            services.AddHttpContextAccessor();

            return services;
        }

    }
}
