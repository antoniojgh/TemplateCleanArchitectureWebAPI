using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita
{
    public class ConsultaObtenerDetalleCita : IRequest<CitaDetalleDTO>
    {
        public required Guid Id { get; set; }
    }
}
