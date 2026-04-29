using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio
{
    public class ConsultaObtenerDetalleConsultorio : IRequest<ConsultorioDetalleDTO>
    {
        public Guid Id { get; set; }
    }
}
