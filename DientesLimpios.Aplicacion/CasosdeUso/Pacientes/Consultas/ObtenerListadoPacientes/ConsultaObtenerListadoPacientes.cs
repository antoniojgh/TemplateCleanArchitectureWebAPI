using DientesLimpios.Aplicacion.Utilidades.Comunes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes
{
    public class ConsultaObtenerListadoPacientes : FiltroPacienteDTO, IRequest<PaginadoDTO<PacienteListadoDTO>>
    {
    }
}
