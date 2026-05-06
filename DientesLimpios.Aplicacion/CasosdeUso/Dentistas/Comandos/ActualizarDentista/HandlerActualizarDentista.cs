using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.ActualizarDentista
{
    public class HandlerActualizarDentista(IRepositorioDentistas repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerActualizarDentista> logger) : IRequestHandler<ComandoActualizarDentista, Result>
    {
        public async Task<Result> Handle(ComandoActualizarDentista request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Actualizando dentista con ID: {DentistaId}", request.Id);

            var dentista = await repositorio.ObtenerPorId(request.Id);

            if (dentista is null)
                return Result.Failure(DomainErrors.Dentista.NoEncontrado);

            var actualizarNombreResult = dentista.ActualizarNombre(request.Nombre);
            if (actualizarNombreResult.IsFailure)
                return actualizarNombreResult;

            var actualizarEmailResult = dentista.ActualizarEmail(request.Email);
            if (actualizarEmailResult.IsFailure)
                return actualizarEmailResult;

            await repositorio.Actualizar(dentista);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Dentista actualizado correctamente con ID: {DentistaId}", request.Id);

            return Result.Success();
        }
    }
}
