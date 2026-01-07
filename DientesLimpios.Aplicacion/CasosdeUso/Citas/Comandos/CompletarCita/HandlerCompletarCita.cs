using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita
{
    public class CasoDeUsoCompletarCita(IRepositorioCitas repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoCompletarCita>
    {

        public async Task Handle(ComandoCompletarCita request, CancellationToken cancellationToken)
        {
            var cita = await repositorio.ObtenerPorId(request.Id);

            if (cita is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            cita.Completar();

            try
            {
                await repositorio.Actualizar(cita);
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
