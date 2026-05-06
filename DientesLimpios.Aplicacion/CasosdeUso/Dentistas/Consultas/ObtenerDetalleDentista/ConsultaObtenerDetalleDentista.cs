using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista
{
    public class ConsultaObtenerDetalleDentista : IRequest<Result<DentistaDetalleDTO>>
    {
        public required Guid Id { get; set; }
    }
}
