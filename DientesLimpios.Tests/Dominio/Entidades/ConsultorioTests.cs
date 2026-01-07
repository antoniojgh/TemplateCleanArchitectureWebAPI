using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class ConsultorioTests
    {
        [Fact]
        public void Constructor_NombreNulo_LanzaExcepcion()
        {
            // Arrange
            Action act = () => new Consultorio(null!);

            // Act & Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }

        [Fact]
        public void Constructor_NombreValido_CreaInstanciaCorrecta()
        {
            // Act
            var consultorio = new Consultorio("Consultorio Central");

            // Assert
            consultorio.Should().NotBeNull();
            consultorio.Nombre.Should().Be("Consultorio Central");
            consultorio.Id.Should().NotBeEmpty();
        }
    }
}
