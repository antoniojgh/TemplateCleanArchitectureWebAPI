using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente
{
    public class HandlerBorrarPaciente(IRepositorioPacientes repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoBorrarPaciente>
    {
        public async Task Handle(ComandoBorrarPaciente request, CancellationToken cancellationToken)
        {
            var paciente = await repositorio.ObtenerPorId(request.Id);

            if (paciente is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            try
            {
                await repositorio.Borrar(paciente);
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
