using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.ActualizarPaciente
{
    public class HandlerActualizarPaciente(IRepositorioPacientes repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerActualizarPaciente> logger) : IRequestHandler<ComandoActualizarPaciente, Result>
    {
        public async Task<Result> Handle(ComandoActualizarPaciente request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Actualizando paciente con ID: {PacienteId}", request.Id);

            var paciente = await repositorio.ObtenerPorId(request.Id);

            if (paciente is null)
                return Result.Failure(DomainErrors.Paciente.NoEncontrado);

            var actualizarNombreResult = paciente.ActualizarNombre(request.Nombre);
            if (actualizarNombreResult.IsFailure)
                return actualizarNombreResult;

            var actualizarEmailResult = paciente.ActualizarEmail(request.Email);
            if (actualizarEmailResult.IsFailure)
                return actualizarEmailResult;

            await repositorio.Actualizar(paciente);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Paciente actualizado correctamente con ID: {PacienteId}", request.Id);

            return Result.Success();

        }
    }
}
