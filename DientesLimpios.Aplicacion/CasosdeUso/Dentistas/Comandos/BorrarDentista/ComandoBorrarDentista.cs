using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista
{
    public class ComandoBorrarDentista : IRequest
    {
        public Guid Id { get; set; }
    }
}
