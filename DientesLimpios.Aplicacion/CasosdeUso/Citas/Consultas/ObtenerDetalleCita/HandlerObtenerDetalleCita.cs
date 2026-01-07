using AutoMapper;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita
{
    public class HandlerObtenerDetalleCita(IRepositorioCitas repositorio, IMapper mapper) : IRequestHandler<ConsultaObtenerDetalleCita, CitaDetalleDTO>
    {
        public async Task<CitaDetalleDTO> Handle(ConsultaObtenerDetalleCita request, CancellationToken cancellationToken)
        {
            var cita = await repositorio.ObtenerPorId(request.Id);

            if (cita is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            var citaDetalleDTO = mapper.Map<CitaDetalleDTO>(cita);

            return citaDetalleDTO;
        }
    }
}
