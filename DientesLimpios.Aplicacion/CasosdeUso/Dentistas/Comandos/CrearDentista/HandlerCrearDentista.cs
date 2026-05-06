using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Entidades;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista
{
    public class HandlerCrearDentista(IRepositorioDentistas repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerCrearDentista> logger) : IRequestHandler<ComandoCrearDentista, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(ComandoCrearDentista request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creando dentista con Nombre {Nombre} y Email {Email}", request.Nombre, request.Email);

            var dentistaResult = Dentista.Crear(request.Nombre, request.Email);

            if (dentistaResult.IsFailure)
                return Result.Failure<Guid>(dentistaResult.Error);

            var dentista = dentistaResult.Value;

            await repositorio.Agregar(dentista);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Dentista creado correctamente con ID: {DentistaId}", dentista.Id);

            return Result.Success(dentista.Id);
        }
    }
}
