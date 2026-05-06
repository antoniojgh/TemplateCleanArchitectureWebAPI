using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio
{
    public class ComandoActualizarConsultorio : IRequest<Result>
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
    }
}
