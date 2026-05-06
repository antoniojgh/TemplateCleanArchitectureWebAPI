using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas
{
    public class HandlerObtenerListadoCitas(IRepositorioCitas repositorio, ILogger<HandlerObtenerListadoCitas> logger) : IRequestHandler<ConsultaObtenerListadoCitas, Result<List<CitaListadoDTO>>>
    {
        public async Task<Result<List<CitaListadoDTO>>> Handle(ConsultaObtenerListadoCitas request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo listado de citas");

            var citas = await repositorio.ObtenerFiltrado(request);

            var citasDTO = citas.Select(cita => cita.ADto()).ToList();

            logger.LogInformation("Listado de citas obtenido correctamente con {NumeroCitas} citas", citasDTO.Count);

            return Result.Success(citasDTO);
        }
    }
}
