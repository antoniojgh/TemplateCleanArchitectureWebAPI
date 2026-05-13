using FluentAssertions;
using NetArchTest.Rules;

namespace DientesLimpios.ArchitectureTests
{
    public class ArchitectureTests
    {
        private const string DomainNamespace = "DientesLimpios.Domain";
        private const string ApplicationNamespace = "DientesLimpios.Application";
        private const string InfrastructureNamespace = "DientesLimpios.Infrastructure";
        private const string IdentityNamespace = "DientesLimpios.Identity";
        private const string PersistenceNamespace = "DientesLimpios.Persistence";
        private const string ApiNamespace = "DientesLimpios.API";


        #region Tests for dependencies between projects

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Domain.Entities.Appointment).Assembly;

            var otherProjects = new[]
            {
                ApplicationNamespace,
                InfrastructureNamespace,
                IdentityNamespace,
                PersistenceNamespace,
                ApiNamespace
            };

            //Act
            var testsResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            testsResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Application_Should_Not_HaveDependencyOnOutwardProjects()
        {
            // Arrange
            var assembly = typeof(Application.Interfaces.Identity.IUserService).Assembly;

            var otherProjects = new[]
            {
                InfrastructureNamespace,
                IdentityNamespace,
                PersistenceNamespace,
                ApiNamespace
            };

            //Act
            var testsResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            testsResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Infraestructure_Identity_Should_Not_HaveDependencyOnOutwardProjects()
        {
            // Arrange
            var assembly = typeof(Identity.Models.User).Assembly;

            var otherProjects = new[]
            {
                ApiNamespace
            };

            //Act
            var testsResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            testsResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Infraestructure_Infrastructure_Should_Not_HaveDependencyOnOutwardProjects()
        {
            // Arrange
            var assembly = typeof(Infrastructure.Notifications.EmailService).Assembly;

            var otherProjects = new[]
            {
                ApiNamespace
            };

            //Act
            var testsResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            testsResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Infraestructure_Persistence_Should_Not_HaveDependencyOnOutwardProjects()
        {
            // Arrange
            var assembly = typeof(Persistence.Configurations.AppointmentConfig).Assembly;

            var otherProjects = new[]
            {
                ApiNamespace
            };

            //Act
            var testsResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            testsResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnInfrastructureLibraries()
        {
            var assembly = typeof(Domain.Entities.Appointment).Assembly;

            var forbiddenLibraries = new[]
            {
                "Microsoft.EntityFrameworkCore",
                "MediatR",
                "Microsoft.AspNetCore",
                "FluentValidation"
            };

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenLibraries)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Persistence_Should_Not_HaveDependencyOn_Infrastructure()
        {
            var assembly = typeof(Persistence.Configurations.AppointmentConfig).Assembly;

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn(InfrastructureNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        #endregion

        #region Tests for dependencies Application Handlers towards Domain

        [Fact]
        public void Handlers_Should_Have_DependencyOnDomain()
        {
            // Arrange
            var assembly = typeof(Application.Interfaces.Identity.IUserService).Assembly;

            //Act
            var testsResult = Types
                .InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Handler", System.StringComparison.Ordinal)
                .Should()
                .HaveDependencyOn(DomainNamespace)
                .GetResult();

            // Assert
            testsResult.IsSuccessful.Should().BeTrue();
        }

        #endregion
    }
}
