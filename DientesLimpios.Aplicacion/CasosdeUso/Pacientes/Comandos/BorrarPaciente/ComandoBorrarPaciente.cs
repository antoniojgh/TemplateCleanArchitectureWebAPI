using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente
{
    public class ComandoBorrarPaciente : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
