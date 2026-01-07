using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class PacienteTests
    {
        [Fact]
        public void Constructor_NombreNulo_LanzaExcepcion()
        {
            // Arrange
            var email = new Email("felipe@ejemplo.com");
            Action act = () => new Paciente(null!, email);

            // Act & Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }

        [Fact]
        public void Constructor_EmailNulo_LanzaExcepcion()
        {
            // Arrange
            Email email = null!;
            Action act = () => new Paciente("Felipe", email);

            // Act & Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }
    }
}
