using DientesLimpios.Dominio.Errores;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.ObjetosDeValor
{
    public class IntervaloDeTiempoTests
    {
        [Fact]
        public void Crear_FechaInicioPosteriorAFin_RetornaFailureInicioMayorOIgualAFin()
        {
            var ahora = DateTime.UtcNow;

            // Act
            var result = IntervaloDeTiempo.Crear(ahora, ahora.AddDays(-1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.IntervaloDeTiempo.InicioMayorOIgualAFin);
        }

        [Fact]
        public void Crear_FechaInicioIgualAFin_RetornaFailureInicioMayorOIgualAFin()
        {
            var ahora = DateTime.UtcNow;

            // Act
            var result = IntervaloDeTiempo.Crear(ahora, ahora);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.IntervaloDeTiempo.InicioMayorOIgualAFin);
        }

        [Fact]
        public void Crear_ParametrosCorrectos_CreaInstanciaCorrecta()
        {
            // Arrange
            var inicio = DateTime.UtcNow;
            var fin = inicio.AddMinutes(30);

            // Act
            var result = IntervaloDeTiempo.Crear(inicio, fin);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var intervalo = result.Value;
            intervalo.Inicio.Should().Be(inicio);
            intervalo.Fin.Should().Be(fin);
        }
    }
}
