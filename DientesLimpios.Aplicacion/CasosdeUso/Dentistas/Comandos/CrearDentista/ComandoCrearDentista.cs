using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista
{
    public class ComandoCrearDentista : IRequest<Result<Guid>>
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
    }
}
