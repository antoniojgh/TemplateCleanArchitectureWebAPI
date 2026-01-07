using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.ActualizarPaciente
{
    public class ComandoActualizarPaciente : IRequest
    {
        public required Guid Id { get; set; }
        public required string Nombre { get; set; }
        public required string Email { get; set; }
    }
}
