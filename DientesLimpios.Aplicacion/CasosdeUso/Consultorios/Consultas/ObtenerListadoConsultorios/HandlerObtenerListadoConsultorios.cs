using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios
{
    public class HandlerObtenerListadoConsultorios(IRepositorioConsultorios repositorio) : IRequestHandler<ConsultaObtenerListadoConsultorios, List<ConsultorioListadoDTO>>
    {
        public async Task<List<ConsultorioListadoDTO>> Handle(ConsultaObtenerListadoConsultorios request, CancellationToken cancellationToken)
        {
            var consultorios = await repositorio.ObtenerTodos();
            var consultoriosDTO = consultorios.Select(consultorio => consultorio.ADto()).ToList();
            return consultoriosDTO;
        }
    }
}
