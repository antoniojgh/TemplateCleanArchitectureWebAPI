using AutoMapper;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Comunes;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes
{
    public class HandlerObtenerListadoPacientes(IRepositorioPacientes repositorio, IMapper mapper) : IRequestHandler<ConsultaObtenerListadoPacientes, PaginadoDTO<PacienteListadoDTO>>
    {
        public async Task<PaginadoDTO<PacienteListadoDTO>> Handle(ConsultaObtenerListadoPacientes request, CancellationToken cancellationToken)
        {
            var pacientesFiltrado = await repositorio.ObtenerFiltrado(request);
            var totalPacientes = await repositorio.ObtenerCantidadTotalRegistros();

            var pacientesFiltradoDTO = mapper.Map<List<PacienteListadoDTO>>(pacientesFiltrado);

            var pacientesDTO = new PaginadoDTO<PacienteListadoDTO>
            {
                Elementos = pacientesFiltradoDTO,
                Total = totalPacientes
            };


            return pacientesDTO;
        }
    }
}
