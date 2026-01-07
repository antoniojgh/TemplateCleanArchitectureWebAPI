using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente
{
   public class ComandoCrearPaciente : IRequest<Guid>
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
    }
}
