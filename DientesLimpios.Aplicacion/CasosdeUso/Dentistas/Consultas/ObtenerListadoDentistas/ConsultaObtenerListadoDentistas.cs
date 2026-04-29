using DientesLimpios.Aplicacion.Utilidades.Comunes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas
{
    public class ConsultaObtenerListadoDentistas : FiltroDentistaDTO, IRequest<PaginadoDTO<DentistaListadoDTO>>
    {
    }
}
