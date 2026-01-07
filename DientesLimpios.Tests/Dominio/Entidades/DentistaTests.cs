using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class DentistaTests
    {

        [Fact]
        public void Constructor_NombreNulo_LanzaExcepcion()
        {
            // Arrange
            var email = new Email("felipe@ejemplo.com");
            Action act = () => new Dentista(null!, email);

            // Act & Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();

        }

        [Fact]
        public void Constructor_EmailNulo_LanzaExcepcion()
        {
            // Arrange
            Email email = null!;
            Action act = () => new Dentista("Felipe", email);

            // Act & Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }
    }
}
