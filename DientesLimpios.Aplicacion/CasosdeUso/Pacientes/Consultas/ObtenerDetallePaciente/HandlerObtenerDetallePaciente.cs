using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerDetallePaciente
{
    public class HandlerObtenerDetallePaciente(IRepositorioPacientes repositorio, ILogger<HandlerObtenerDetallePaciente> logger) : IRequestHandler<ConsultaObtenerDetallePaciente, Result<PacienteDetalleDTO>>
    {
        public async Task<Result<PacienteDetalleDTO>> Handle(ConsultaObtenerDetallePaciente request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo detalle de paciente con ID: {PacienteId}", request.Id);

            var paciente = await repositorio.ObtenerPorId(request.Id);

            if (paciente is null)
                return Result.Failure<PacienteDetalleDTO>(DomainErrors.Paciente.NoEncontrado);

            logger.LogInformation("Detalle de paciente obtenido correctamente con ID: {PacienteId}", request.Id);

            return Result.Success(paciente.ADto());
        }
    }
}
