using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Application.Interfaces.Persistence
{
    public interface IUnitOfWork
    {
        Task SaveChanges();
        Task Reversar();
    }
}
