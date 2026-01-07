using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.ObjetosDeValor;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.ActualizarDentista
{
    public class HandlerActualizarDentista(IRepositorioDentistas repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoActualizarDentista>
    {
        public async Task Handle(ComandoActualizarDentista request, CancellationToken cancellationToken)
        {
            var dentista = await repositorio.ObtenerPorId(request.Id);

            if (dentista is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            dentista.ActualizarNombre(request.Nombre);
            var email = new Email(request.Email);
            dentista.ActualizarEmail(email);

            try
            {
                await repositorio.Actualizar(dentista);
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
