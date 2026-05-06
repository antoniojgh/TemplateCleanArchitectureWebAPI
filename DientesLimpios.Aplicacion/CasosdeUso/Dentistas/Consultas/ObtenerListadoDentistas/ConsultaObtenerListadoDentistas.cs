using DientesLimpios.Aplicacion.Utilidades.Comunes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas
{
    public class ConsultaObtenerListadoDentistas : FiltroDentistaDTO, IRequest<Result<PaginadoDTO<DentistaListadoDTO>>>
    {
    }
}
