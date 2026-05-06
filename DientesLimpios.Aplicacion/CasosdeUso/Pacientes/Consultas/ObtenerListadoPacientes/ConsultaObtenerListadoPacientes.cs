using DientesLimpios.Aplicacion.Utilidades.Comunes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes
{
    public class ConsultaObtenerListadoPacientes : FiltroPacienteDTO, IRequest<Result<PaginadoDTO<PacienteListadoDTO>>>
    {
    }
}
