using DientesLimpios.Application.Interfaces.Identity;
using DientesLimpios.Identity.Models;
using DientesLimpios.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DientesLimpios.Identity
{
    public static class IdentityServiceRegistration
    {
        public static void AgregarServicesDeIdentity(this IServiceCollection services)
        {
            services.AddAuthentication(IdentityConstants.BearerScheme).AddBearerToken(IdentityConstants.BearerScheme);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("esadmin", policy => policy.RequireClaim("esadmin"));
            });

            services.AddDbContext<DientesLimpiosIdentityDbContext>(options =>
            options.UseSqlServer("name=DientesLimpiosConnectionString"));

            services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<DientesLimpiosIdentityDbContext>()
                .AddApiEndpoints();

            services.AddTransient<IUserService, UserService>();
            services.AddHttpContextAccessor();
        }

    }
}
