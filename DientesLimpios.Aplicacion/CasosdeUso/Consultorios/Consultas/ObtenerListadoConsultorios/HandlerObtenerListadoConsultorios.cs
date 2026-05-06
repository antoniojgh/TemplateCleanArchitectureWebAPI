using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios
{
    public class HandlerObtenerListadoConsultorios(IRepositorioConsultorios repositorio, ILogger<HandlerObtenerListadoConsultorios> logger) : IRequestHandler<ConsultaObtenerListadoConsultorios, Result<List<ConsultorioListadoDTO>>>
    {
        public async Task<Result<List<ConsultorioListadoDTO>>> Handle(ConsultaObtenerListadoConsultorios request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo listado de consultorios");

            var consultorios = await repositorio.ObtenerTodos();
            var consultoriosDTO = consultorios.Select(consultorio => consultorio.ADto()).ToList();

            logger.LogInformation("Listado de consultorios obtenido correctamente con {NumeroConsultorios} consultorios", consultoriosDTO.Count);

            return Result.Success(consultoriosDTO);
        }
    }
}
