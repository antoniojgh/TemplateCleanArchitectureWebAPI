using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita
{
    public class ComandoCancelarCita : IRequest
    {
        public required Guid Id { get; set; }
    }
}
