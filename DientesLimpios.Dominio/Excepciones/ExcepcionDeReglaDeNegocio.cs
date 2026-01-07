using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Dominio.Excepciones
{
    public class ExcepcionDeReglaDeNegocio : Exception
    {
        public ExcepcionDeReglaDeNegocio(string mensaje) : base(mensaje)
        {

        }
    }
}
