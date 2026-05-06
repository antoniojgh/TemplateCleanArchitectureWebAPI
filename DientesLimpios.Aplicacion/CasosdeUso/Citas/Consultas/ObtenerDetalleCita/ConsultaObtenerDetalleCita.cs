using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita
{
    public class ConsultaObtenerDetalleCita : IRequest<Result<CitaDetalleDTO>>
    {
        public required Guid Id { get; set; }
    }
}
