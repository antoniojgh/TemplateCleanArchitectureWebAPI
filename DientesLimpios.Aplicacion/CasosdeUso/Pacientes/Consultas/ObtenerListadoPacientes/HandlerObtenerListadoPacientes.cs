using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Comunes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes
{
    public class HandlerObtenerListadoPacientes(IRepositorioPacientes repositorio) : IRequestHandler<ConsultaObtenerListadoPacientes, PaginadoDTO<PacienteListadoDTO>>
    {
        public async Task<PaginadoDTO<PacienteListadoDTO>> Handle(ConsultaObtenerListadoPacientes request)
        {
            var pacientesFiltrado = await repositorio.ObtenerFiltrado(request);
            var totalPacientes = await repositorio.ObtenerCantidadTotalRegistros();

            var pacientesFiltradoDTO = pacientesFiltrado.Select(paciente => paciente.ADto()).ToList(); ;

            var pacientesDTO = new PaginadoDTO<PacienteListadoDTO>
            {
                Elementos = pacientesFiltradoDTO,
                Total = totalPacientes
            };


            return pacientesDTO;
        }
    }
}
