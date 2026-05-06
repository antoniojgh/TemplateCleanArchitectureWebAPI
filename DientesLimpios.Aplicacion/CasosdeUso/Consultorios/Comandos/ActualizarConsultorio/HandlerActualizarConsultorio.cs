using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio
{
    public class HandlerActualizarConsultorio(IRepositorioConsultorios repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerActualizarConsultorio> logger) : IRequestHandler<ComandoActualizarConsultorio, Result>
    {
        public async Task<Result> Handle(ComandoActualizarConsultorio request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Actualizando consultorio con ID: {ConsultorioId}", request.Id);

            var consultorio = await repositorio.ObtenerPorId(request.Id);

            if (consultorio is null)
                return Result.Failure(DomainErrors.Consultorio.NoEncontrado);

            var actualizarResult = consultorio.ActualizarNombre(request.Nombre);
            if (actualizarResult.IsFailure)
                return actualizarResult;

            await repositorio.Actualizar(consultorio);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Consultorio actualizado correctamente con ID: {ConsultorioId}", request.Id);

            return Result.Success();

        }
    }
}
