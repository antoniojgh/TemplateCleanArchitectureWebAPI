using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Entidades;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio
{
    public class HandlerCrearConsultorio(IRepositorioConsultorios repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerCrearConsultorio> logger) : IRequestHandler<ComandoCrearConsultorio, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(ComandoCrearConsultorio request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creando consultorio con nombre: {Nombre}", request.Nombre);

            var consultorioResult = Consultorio.Crear(request.Nombre);

            if (consultorioResult.IsFailure)
                return Result.Failure<Guid>(consultorioResult.Error);

            var consultorio = consultorioResult.Value;

            var respuesta = await repositorio.Agregar(consultorio);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Consultorio creado correctamente con nombre: {Nombre}", request.Nombre);

            return Result.Success(respuesta.Id);

        }
    }
}
