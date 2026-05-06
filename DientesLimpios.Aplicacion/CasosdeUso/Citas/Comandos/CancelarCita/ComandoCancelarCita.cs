using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CancelarCita
{
    public class ComandoCancelarCita : IRequest<Result>
    {
        public required Guid Id { get; set; }
    }
}
