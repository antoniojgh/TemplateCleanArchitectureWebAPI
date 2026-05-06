using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Entidades;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente
{
    public class HandlerCrearPaciente(IRepositorioPacientes repositorio, IUnitOfwork unidadDeTrabajo, ILogger<HandlerCrearPaciente> logger) : IRequestHandler<ComandoCrearPaciente, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(ComandoCrearPaciente request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creando paciente con Nombre {Nombre} y Email {Email}", request.Nombre, request.Email);

            var pacienteResult = Paciente.Crear(request.Nombre, request.Email);

            if (pacienteResult.IsFailure)
                return Result.Failure<Guid>(pacienteResult.Error);

            var paciente = pacienteResult.Value;

            await repositorio.Agregar(paciente);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Paciente creado correctamente con ID: {PacienteId}", paciente.Id);

            return Result.Success(paciente.Id);

        }
    }
}
