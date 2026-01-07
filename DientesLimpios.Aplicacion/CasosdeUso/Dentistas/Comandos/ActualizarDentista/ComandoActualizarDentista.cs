using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.ActualizarDentista
{
    public class ComandoActualizarDentista : IRequest
    {
        public required Guid Id { get; set; }
        public required string Nombre { get; set; }
        public required string Email { get; set; }
    }
}
