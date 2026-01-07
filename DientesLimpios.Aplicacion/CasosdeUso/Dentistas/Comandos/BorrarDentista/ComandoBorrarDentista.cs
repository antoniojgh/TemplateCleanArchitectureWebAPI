using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista
{
    public class ComandoBorrarDentista : IRequest
    {
        public Guid Id { get; set; }
    }
}
