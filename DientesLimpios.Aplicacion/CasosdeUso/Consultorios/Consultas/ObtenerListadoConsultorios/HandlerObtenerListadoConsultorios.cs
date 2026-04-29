using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios
{
    public class HandlerObtenerListadoConsultorios(IRepositorioConsultorios repositorio) : IRequestHandler<ConsultaObtenerListadoConsultorios, List<ConsultorioListadoDTO>>
    {
        public async Task<List<ConsultorioListadoDTO>> Handle(ConsultaObtenerListadoConsultorios request)
        {
            var consultorios = await repositorio.ObtenerTodos();
            var consultoriosDTO = consultorios.Select(consultorio => consultorio.ADto()).ToList();
            return consultoriosDTO;
        }
    }
}
