using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;

namespace DientesLimpios.Persistencia.UnidadesDeTrabajo
{
    public class UnidadDeTrabajoEFCore(DientesLimpiosDbContext context) : IUnitOfwork
    {
        public async Task Persistir()
        {
            await context.SaveChangesAsync();
        }

        public Task Reversar()
        {
            return Task.CompletedTask;
        }
    }
}
