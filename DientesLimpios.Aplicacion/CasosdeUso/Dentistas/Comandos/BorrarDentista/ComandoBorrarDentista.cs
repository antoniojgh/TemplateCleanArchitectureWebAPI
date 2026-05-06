using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista
{
    public class ComandoBorrarDentista : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
