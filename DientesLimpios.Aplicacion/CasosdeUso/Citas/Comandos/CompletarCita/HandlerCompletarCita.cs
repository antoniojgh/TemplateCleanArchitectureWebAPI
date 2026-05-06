using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita
{
    public class HandlerCompletarCita(IRepositorioCitas repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerCompletarCita> logger) : IRequestHandler<ComandoCompletarCita, Result>
    {

        public async Task<Result> Handle(ComandoCompletarCita request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Completando cita con ID: {CitaId}", request.Id);

            var cita = await repositorio.ObtenerPorId(request.Id);

            if (cita is null)
                return Result.Failure(DomainErrors.Cita.NoEncontrada);

            var completarResult = cita.Completar();
            if (completarResult.IsFailure)
                return completarResult;

            await repositorio.Actualizar(cita);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Cita completada correctamente con ID: {CitaId}", request.Id);

            return Result.Success();
        }
    }
}
