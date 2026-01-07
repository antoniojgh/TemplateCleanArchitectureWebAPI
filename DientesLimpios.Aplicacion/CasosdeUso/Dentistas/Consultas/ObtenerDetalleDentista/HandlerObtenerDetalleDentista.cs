using AutoMapper;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista
{
    public class HandlerObtenerDetalleDentista(IRepositorioDentistas repositorio, IMapper mapper) : IRequestHandler<ConsultaObtenerDetalleDentista, DentistaDetalleDTO>
    {
        public async Task<DentistaDetalleDTO> Handle(ConsultaObtenerDetalleDentista request, CancellationToken cancellationToken)
        {
            var dentista = await repositorio.ObtenerPorId(request.Id);

            if (dentista is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            var dentistaDetalleDTO = mapper.Map<DentistaDetalleDTO>(dentista);

            return dentistaDetalleDTO;
        }
    }
}
