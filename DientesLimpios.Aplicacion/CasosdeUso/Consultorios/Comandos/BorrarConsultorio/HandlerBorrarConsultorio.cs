using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio
{
    public class HandlerBorrarConsultorio(IRepositorioConsultorios repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerBorrarConsultorio> logger) : IRequestHandler<ComandoBorrarConsultorio, Result>
    {
        public async Task<Result> Handle(ComandoBorrarConsultorio request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Borrando consultorio con ID: {ConsultorioId}", request.Id);

            var consultorio = await repositorio.ObtenerPorId(request.Id);

            if (consultorio is null)
                return Result.Failure(DomainErrors.Consultorio.NoEncontrado);

            await repositorio.Borrar(consultorio);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Consultorio borrado correctamente con ID: {ConsultorioId}", request.Id);

            return Result.Success();
        }
    }
}
