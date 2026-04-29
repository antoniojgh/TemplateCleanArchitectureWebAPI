using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio
{
    public class ComandoActualizarConsultorio : IRequest
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
    }
}
