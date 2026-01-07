using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor; // Ensure you have the reference to your Domain
using FluentAssertions;
using Xunit; // xUnit namespace

namespace DientesLimpios.Tests.Dominio.ObjetosDeValor
{
    public class EmailTests
    {
        // 1. Testing Null -> Expecting Exception
        [Fact] 
        public void Constructor_EmailNulo_LanzaExcepcion()
        {
            // Arrange
            // We define the action that causes the error
            Action act = () => new Email(null!);

            // Act & Assert
            // FluentAssertions makes this readable
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }

        // 2. Testing Invalid Format -> Expecting Exception
        [Fact]
        public void Constructor_EmailSinArroba_LanzaExcepcion()
        {
            // Arrange
            Action act = () => new Email("felipe.com");

            // Act & Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }

        // 3. Testing Valid Case
        [Fact]
        public void Constructor_EmailValido_NoLanzaExcepcion()
        {
            // Act
            var email = new Email("felipe@ejemplo.com");

            // Assert
            email.Should().NotBeNull();
            email.Valor.Should().Be("felipe@ejemplo.com");
        }
    }
}
