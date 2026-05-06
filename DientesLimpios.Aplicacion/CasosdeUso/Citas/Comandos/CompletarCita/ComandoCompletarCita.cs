using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita
{
    public class ComandoCompletarCita : IRequest<Result>
    {
        public required Guid Id { get; set; }
    }
}
