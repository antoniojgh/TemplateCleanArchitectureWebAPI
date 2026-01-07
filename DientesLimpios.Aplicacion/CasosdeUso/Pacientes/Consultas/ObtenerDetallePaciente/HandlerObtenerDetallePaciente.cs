using AutoMapper;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerDetallePaciente
{
    public class HandlerObtenerDetallePaciente(IRepositorioPacientes repositorio, IMapper mapper) : IRequestHandler<ConsultaObtenerDetallePaciente, PacienteDetalleDTO>
    {
        public async Task<PacienteDetalleDTO> Handle(ConsultaObtenerDetallePaciente request, CancellationToken cancellationToken)
        {
            var paciente = await repositorio.ObtenerPorId(request.Id);

            if (paciente is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            var pacienteDetalleDTO = mapper.Map<PacienteDetalleDTO>(paciente);

            return pacienteDetalleDTO;
        }
    }
}
