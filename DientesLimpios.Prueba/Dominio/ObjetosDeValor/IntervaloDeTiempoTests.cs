using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DientesLimpios.Pruebas.Dominio.ObjetosDeValor
{
    [TestClass]
    public class IntervaloDeTiempoTests
    {
        [TestMethod]
        public void Constructor_FechaInicioPosteriorAFechaFin_LanzaExcepcion()
        {
            Assert.Throws<ExcepcionDeReglaDeNegocio>(() => new IntervaloDeTiempo(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1)));
        }

        [TestMethod]
        public void Constructor_ParametrosCorrectos_NoLanzaExcepcion()
        {
            var intervaloTiempo = new IntervaloDeTiempo(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30));

            // Assert
            Assert.IsNotNull(intervaloTiempo);
        }
    }
}
