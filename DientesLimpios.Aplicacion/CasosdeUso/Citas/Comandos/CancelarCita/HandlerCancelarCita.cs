using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CancelarCita
{
    public class HandlerCancelarCita(IRepositorioCitas repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoCancelarCita>
    {

        public async Task Handle(ComandoCancelarCita request)
        {
            var cita = await repositorio.ObtenerPorId(request.Id);

            if (cita is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            cita.Cancelar();

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
