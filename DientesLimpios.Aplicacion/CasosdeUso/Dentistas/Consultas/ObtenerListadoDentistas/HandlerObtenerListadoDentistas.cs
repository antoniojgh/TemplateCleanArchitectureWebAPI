using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Comunes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas
{
    public class HandlerObtenerListadoDentistas(IRepositorioDentistas repositorio, ILogger<HandlerObtenerListadoDentistas> logger) : IRequestHandler<ConsultaObtenerListadoDentistas, Result<PaginadoDTO<DentistaListadoDTO>>>
    {
        public async Task<Result<PaginadoDTO<DentistaListadoDTO>>> Handle(ConsultaObtenerListadoDentistas request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo listado de dentistas");

            var dentistasFiltrado = await repositorio.ObtenerFiltrado(request);
            var totalDentistas = await repositorio.ObtenerCantidadTotalRegistros();

            var dentistasFiltradoDTO = dentistasFiltrado.Select(dentista => dentista.ADto()).ToList();

            var dentistasDTO = new PaginadoDTO<DentistaListadoDTO>
            {
                Elementos = dentistasFiltradoDTO,
                Total = totalDentistas
            };

            logger.LogInformation("Listado de dentistas obtenido correctamente con {NumeroDentistas} dentistas", dentistasDTO.Elementos.Count);

            return Result.Success(dentistasDTO);
        }
    }
}
