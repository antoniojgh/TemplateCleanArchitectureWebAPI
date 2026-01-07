using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista
{
    public class HandlerBorrarDentista(IRepositorioDentistas repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoBorrarDentista>
    {
        public async Task Handle(ComandoBorrarDentista request, CancellationToken cancellationToken)
        {
            var dentista = await repositorio.ObtenerPorId(request.Id);

            if (dentista is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            try
            {
                await repositorio.Borrar(dentista);
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
