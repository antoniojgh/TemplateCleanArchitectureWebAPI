using DientesLimpios.Aplicacion.Utilidades.Comunes;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes
{
    public class ConsultaObtenerListadoPacientes : FiltroPacienteDTO, IRequest<PaginadoDTO<PacienteListadoDTO>>
    {
    }
}
