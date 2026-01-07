using AutoMapper;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Comunes;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas
{
    public class HandlerObtenerListadoDentistas(IRepositorioDentistas repositorio, IMapper mapper) : IRequestHandler<ConsultaObtenerListadoDentistas, PaginadoDTO<DentistaListadoDTO>>
    {
        public async Task<PaginadoDTO<DentistaListadoDTO>> Handle(ConsultaObtenerListadoDentistas request, CancellationToken cancellationToken)
        {
            var dentistasFiltrado = await repositorio.ObtenerFiltrado(request);
            var totalDentistas = await repositorio.ObtenerCantidadTotalRegistros();

            var dentistasFiltradoDTO = mapper.Map<List<DentistaListadoDTO>>(dentistasFiltrado);

            var dentistasDTO = new PaginadoDTO<DentistaListadoDTO>
            {
                Elementos = dentistasFiltradoDTO,
                Total = totalDentistas
            };

            return dentistasDTO;
        }
    }
}
