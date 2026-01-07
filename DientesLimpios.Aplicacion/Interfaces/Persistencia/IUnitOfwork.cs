using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Aplicacion.Interfaces.Persistencia
{
    public interface IUnitOfwork
    {
        Task Persistir();
        Task Reversar();
    }
}
