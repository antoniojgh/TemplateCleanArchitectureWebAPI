using FluentAssertions;
using NetArchTest.Rules;

namespace DientesLimpios.ArchitectureTests
{
    public class ArchitectureTests
    {
        private const string DomainNamespace = "DientesLimpios.Dominio";
        private const string ApplicationNamespace = "DientesLimpios.Aplicacion";
        private const string InfraestructuraNamespace = "DientesLimpios.Infraestructura";
        private const string IdentidadNamespace = "DientesLimpios.Identidad";
        private const string PersistenciaNamespace = "DientesLimpios.Persistencia";
        private const string ApiNamespace = "DientesLimpios.API";


        #region Tests for dependencies between projects

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Dominio.Entidades.Cita).Assembly;

            var otherProjects = new[]
            {
                ApplicationNamespace,
                InfraestructuraNamespace,
                IdentidadNamespace,
                PersistenciaNamespace,
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
            var assembly = typeof(Aplicacion.Interfaces.Identidad.IServicioUsuarios).Assembly;

            var otherProjects = new[]
            {
                InfraestructuraNamespace,
                IdentidadNamespace,
                PersistenciaNamespace,
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
        public void Infraestructure_Identidad_Should_Not_HaveDependencyOnOutwardProjects()
        {
            // Arrange
            var assembly = typeof(Identidad.Modelos.Usuario).Assembly;

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
        public void Infraestructure_Infraestructura_Should_Not_HaveDependencyOnOutwardProjects()
        {
            // Arrange
            var assembly = typeof(Infraestructura.Notificaciones.ServicioCorreos).Assembly;

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
        public void Infraestructure_Persistencia_Should_Not_HaveDependencyOnOutwardProjects()
        {
            // Arrange
            var assembly = typeof(Persistencia.Configuraciones.CitaConfig).Assembly;

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
            var assembly = typeof(Dominio.Entidades.Cita).Assembly;

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
        public void Persistencia_Should_Not_HaveDependencyOn_Infraestructura()
        {
            var assembly = typeof(Persistencia.Configuraciones.CitaConfig).Assembly;

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn(InfraestructuraNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        #endregion

        #region Tests for dependencies Application Handlers towards Domain

        [Fact]
        public void Handlers_Should_Have_DependencyOnDomain()
        {
            // Arrange
            var assembly = typeof(Aplicacion.Interfaces.Identidad.IServicioUsuarios).Assembly;

            //Act
            var testsResult = Types
                .InAssembly(assembly)
                .That()
                .HaveNameStartingWith("Handler")
                .Should()
                .HaveDependencyOn(DomainNamespace)
                .GetResult();

            // Assert
            testsResult.IsSuccessful.Should().BeTrue();
        }

        #endregion
    }
}
