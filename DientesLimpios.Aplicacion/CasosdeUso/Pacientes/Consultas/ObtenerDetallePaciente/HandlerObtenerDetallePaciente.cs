using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerDetallePaciente
{
    public class HandlerObtenerDetallePaciente(IRepositorioPacientes repositorio) : IRequestHandler<ConsultaObtenerDetallePaciente, PacienteDetalleDTO>
    {
        public async Task<PacienteDetalleDTO> Handle(ConsultaObtenerDetallePaciente request)
        {
            var paciente = await repositorio.ObtenerPorId(request.Id);

            if (paciente is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            var pacienteDetalleDTO = paciente.ADto();

            return pacienteDetalleDTO;
        }
    }
}
