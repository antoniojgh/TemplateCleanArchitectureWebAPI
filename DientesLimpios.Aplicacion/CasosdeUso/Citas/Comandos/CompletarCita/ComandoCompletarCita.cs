using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita
{
    public class ComandoCompletarCita : IRequest
    {
        public required Guid Id { get; set; }
    }
}
