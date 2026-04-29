using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio
{
    public class HandlerActualizarConsultorio(IRepositorioConsultorios repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoActualizarConsultorio>
    {
        public async Task Handle(ComandoActualizarConsultorio request)
        {
            var consultorio = await repositorio.ObtenerPorId(request.Id);

            if (consultorio is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            consultorio.ActualizarNombre(request.Nombre);

            try
            {
                await repositorio.Actualizar(consultorio);
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
