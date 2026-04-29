using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas
{
    public class HandlerObtenerListadoCitas(IRepositorioCitas repositorio) : IRequestHandler<ConsultaObtenerListadoCitas, List<CitaListadoDTO>>
    {
        public async Task<List<CitaListadoDTO>> Handle(ConsultaObtenerListadoCitas request, CancellationToken cancellationToken)
        {
            var citas = await repositorio.ObtenerFiltrado(request);

            var citasDTO = citas.Select(cita => cita.ADto()).ToList(); ;

            return citasDTO;
        }
    }
}
