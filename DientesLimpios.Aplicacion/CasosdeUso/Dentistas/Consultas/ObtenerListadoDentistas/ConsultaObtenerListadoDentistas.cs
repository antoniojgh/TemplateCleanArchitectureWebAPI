using DientesLimpios.Aplicacion.Utilidades.Comunes;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas
{
    public class ConsultaObtenerListadoDentistas : FiltroDentistaDTO, IRequest<PaginadoDTO<DentistaListadoDTO>>
    {
    }
}
