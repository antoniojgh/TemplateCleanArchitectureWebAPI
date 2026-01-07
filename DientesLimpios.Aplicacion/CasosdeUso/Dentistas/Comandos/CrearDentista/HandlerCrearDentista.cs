using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.ObjetosDeValor;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista
{
    public class HandlerCrearDentista(IRepositorioDentistas repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoCrearDentista, Guid>
    {
        public async Task<Guid> Handle(ComandoCrearDentista request, CancellationToken cancellationToken)
        {
            var email = new Email(request.Email);
            var dentista = new Dentista(request.Nombre, email);

            try
            {
                var respuesta = await repositorio.Agregar(dentista);
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
