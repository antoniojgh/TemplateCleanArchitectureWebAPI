using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Comunes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes
{
    public class HandlerObtenerListadoPacientes(IRepositorioPacientes repositorio, ILogger<HandlerObtenerListadoPacientes> logger) : IRequestHandler<ConsultaObtenerListadoPacientes, Result<PaginadoDTO<PacienteListadoDTO>>>
    {
        public async Task<Result<PaginadoDTO<PacienteListadoDTO>>> Handle(ConsultaObtenerListadoPacientes request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo listado de pacientes");

            var pacientesFiltrado = await repositorio.ObtenerFiltrado(request);
            var totalPacientes = await repositorio.ObtenerCantidadTotalRegistros();

            var pacientesFiltradoDTO = pacientesFiltrado.Select(paciente => paciente.ADto()).ToList(); ;

            var pacientesDTO = new PaginadoDTO<PacienteListadoDTO>
            {
                Elementos = pacientesFiltradoDTO,
                Total = totalPacientes
            };

            logger.LogInformation("Listado de pacientes obtenido correctamente con {NumeroPacientes} pacientes", pacientesDTO.Elementos.Count);

            return Result.Success(pacientesDTO);
        }
    }
}
