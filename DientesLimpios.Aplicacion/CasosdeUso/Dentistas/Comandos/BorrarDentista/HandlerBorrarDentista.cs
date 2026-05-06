using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista
{
    public class HandlerBorrarDentista(IRepositorioDentistas repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerBorrarDentista> logger) : IRequestHandler<ComandoBorrarDentista, Result>
    {
        public async Task<Result> Handle(ComandoBorrarDentista request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Borrando dentista con ID: {DentistaId}", request.Id);

            var dentista = await repositorio.ObtenerPorId(request.Id);

            if (dentista is null)
                return Result.Failure(DomainErrors.Dentista.NoEncontrado);

            await repositorio.Borrar(dentista);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Dentista borrado correctamente con ID: {DentistaId}", request.Id);

            return Result.Success();
        }
    }
}
