using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente
{
   public class ComandoCrearPaciente : IRequest<Result<Guid>>
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
    }
}
