using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.ObjetosDeValor
{
    public class IntervaloDeTiempoTests
    {
        [Fact]
        public void Constructor_FechaInicioPosteriorAFechaFin_LanzaExcepcion()
        {
            // Arrange
            // We define the action that causes the error
            Action act = () => new IntervaloDeTiempo(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));

            // Act & Assert
            // FluentAssertions makes this readable
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }

        [Fact]
        public void Constructor_ParametrosCorrectos_NoLanzaExcepcion()
        {
            // Act
            var intervaloTiempo = new IntervaloDeTiempo(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30));

            // Assert
            intervaloTiempo.Should().NotBeNull();
        }
    }
}
