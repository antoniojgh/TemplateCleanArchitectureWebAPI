using DientesLimpios.Persistence;
using DientesLimpios.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

namespace DientesLimpios.IntegrationTests
{
    public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _db = new MsSqlBuilder()
                                                    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                                                    .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                // 1) Point the domain DbContext at the container, keeping the interceptors
                //    (so domain events + auditing behave exactly as in production).
                services.RemoveAll<DbContextOptions<DientesLimpiosDbContext>>();
                services.RemoveAll<DientesLimpiosDbContext>();

                services.AddDbContext<DientesLimpiosDbContext>((sp, options) =>
                {
                    options.UseSqlServer(_db.GetConnectionString());
                    options.AddInterceptors(
                        sp.GetRequiredService<AuditableEntitiesInterceptor>(),
                        sp.GetRequiredService<DispatchDomainEventsInterceptor>());
                });

                // 2) Replace Bearer auth with an always-authenticated test scheme
                //    carrying the "esadmin" claim. ConfigureTestServices runs last,
                //    so this becomes the default scheme.
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

                // 3) Don't run the background reminder job during tests.
                services.RemoveAll<IHostedService>();
            });
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _db.StartAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Could not start the SQL Server test container. Integration tests " +
                    "require Docker to be installed and running. On GitHub Actions " +
                    "(ubuntu-latest) Docker is preinstalled.", ex);
            }

            // The "Testing" environment doesn't load appsettings.Development.json, so the
            // connection string the registrations read eagerly is missing. Supply the
            // container's connection string BEFORE the host is built (the next line builds it).
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__DientesLimpiosConnectionString",
                _db.GetConnectionString());

            using var scope = Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DientesLimpiosDbContext>();
            await ctx.Database.MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _db.DisposeAsync();
            await base.DisposeAsync();
        }
    }

}
