using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CancelarCita
{
    public class ComandoCancelarCita : IRequest
    {
        public required Guid Id { get; set; }
    }
}
