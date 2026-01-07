using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio
{
    public class HandlerBorrarConsultorio(IRepositorioConsultorios repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoBorrarConsultorio>
    {
        public async Task Handle(ComandoBorrarConsultorio request, CancellationToken cancellationToken)
        {
            var consultorio = await repositorio.ObtenerPorId(request.Id);

            if (consultorio is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            try
            {
                await repositorio.Borrar(consultorio);
                await unidadDeTrabajo.Persistir();
            }
            catch (Exception)
            {
                await unidadDeTrabajo.Reversar();
                throw;
            }

        }
    }
}
