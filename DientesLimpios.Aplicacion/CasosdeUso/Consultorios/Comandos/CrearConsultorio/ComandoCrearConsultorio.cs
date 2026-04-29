using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio
{
    public class ComandoCrearConsultorio : IRequest<Guid>
    {
        public required string Nombre { get; set; }
    }
}
