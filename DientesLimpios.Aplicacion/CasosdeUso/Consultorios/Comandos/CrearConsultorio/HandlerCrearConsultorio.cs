using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio
{
    public class HandlerCrearConsultorio(IRepositorioConsultorios repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoCrearConsultorio, Guid>
    {
        // Note: IValidator is NO LONGER injected here. The Behavior handles it.
        public async Task<Guid> Handle(ComandoCrearConsultorio request, CancellationToken cancellationToken)
        {
            // --- Validation Code Removed ---
            // It is now executed automatically before this method is ever called.

            var consultorio = new Consultorio(request.Nombre);

            try
            {
                var respuesta = await repositorio.Agregar(consultorio);
                await unidadDeTrabajo.Persistir();
                return respuesta.Id;
            }
            catch (Exception)
            {
                await unidadDeTrabajo.Reversar();
                throw;
            }
        }
    }
}
