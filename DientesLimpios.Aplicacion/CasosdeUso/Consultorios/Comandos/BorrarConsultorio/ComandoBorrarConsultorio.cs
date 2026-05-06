using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio
{
    public class ComandoBorrarConsultorio : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
