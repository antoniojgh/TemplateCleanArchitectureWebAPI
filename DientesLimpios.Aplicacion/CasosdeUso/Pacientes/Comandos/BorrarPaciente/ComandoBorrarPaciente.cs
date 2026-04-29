using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente
{
    public class ComandoBorrarPaciente : IRequest
    {
        public Guid Id { get; set; }
    }
}
