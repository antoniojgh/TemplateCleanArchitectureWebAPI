using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CancelarCita
{
    public class HandlerCancelarCita(IRepositorioCitas repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerCancelarCita> logger) : IRequestHandler<ComandoCancelarCita, Result>
    {

        public async Task<Result> Handle(ComandoCancelarCita request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Cancelando cita con ID: {CitaId}", request.Id);

            var cita = await repositorio.ObtenerPorId(request.Id);

            if (cita is null)
                return Result.Failure(DomainErrors.Cita.NoEncontrada);

            var cancelarResult = cita.Cancelar();
            if (cancelarResult.IsFailure)
                return cancelarResult;

            await repositorio.Actualizar(cita);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Cita cancelada correctamente con ID: {CitaId}", request.Id);

            return Result.Success();

        }
    }
}
