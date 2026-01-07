using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista
{
    public class ConsultaObtenerDetalleDentista : IRequest<DentistaDetalleDTO>
    {
        public required Guid Id { get; set; }
    }
}
