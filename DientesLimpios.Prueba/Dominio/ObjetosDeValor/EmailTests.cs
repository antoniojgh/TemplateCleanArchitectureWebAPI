using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DientesLimpios.Pruebas.Dominio.ObjetosDeValor
{
    [TestClass]
    public class EmailTests
    {
        [TestMethod]
        public void Constructor_EmailNulo_LanzaExcepcion()
        {
            Assert.Throws<ExcepcionDeReglaDeNegocio>(() => new Email(null!));
        }

        [TestMethod]
        public void Constructor_EmailSinArroba_LanzaExcepcion()
        {
            Assert.Throws<ExcepcionDeReglaDeNegocio>(() => new Email("felipe.com"));
        }

        [TestMethod]
        public void Constructor_EmailValido_NoLanzaExcepcion()
        {
            var email = new Email("felipe@ejemplo.com");

            // Assert
            Assert.IsNotNull(email);
        }


    }
}
