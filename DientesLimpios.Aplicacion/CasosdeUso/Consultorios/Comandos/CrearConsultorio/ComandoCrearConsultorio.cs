using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio
{
    public class ComandoCrearConsultorio : IRequest<Result<Guid>>
    {
        public required string Nombre { get; set; }
    }
}
