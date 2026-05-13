using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Application.Interfaces.Persistence;

namespace DientesLimpios.Persistence.UnitsOfWork
{
    public class EFCoreUnitOfWork(DientesLimpiosDbContext context) : IUnitOfWork
    {
        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }

        public Task Reversar()
        {
            return Task.CompletedTask;
        }
    }
}
