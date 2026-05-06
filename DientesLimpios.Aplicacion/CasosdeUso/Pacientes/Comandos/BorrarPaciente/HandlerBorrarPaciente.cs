using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente
{
    public class HandlerBorrarPaciente(IRepositorioPacientes repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerBorrarPaciente> logger) : IRequestHandler<ComandoBorrarPaciente, Result>
    {
        public async Task<Result> Handle(ComandoBorrarPaciente request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Borrando paciente con ID: {PacienteId}", request.Id);

            var paciente = await repositorio.ObtenerPorId(request.Id);

            if (paciente is null)
                return Result.Failure(DomainErrors.Paciente.NoEncontrado);

            await repositorio.Borrar(paciente);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Paciente borrado correctamente con ID: {PacienteId}", request.Id);

            return Result.Success();
        }
    }
}
