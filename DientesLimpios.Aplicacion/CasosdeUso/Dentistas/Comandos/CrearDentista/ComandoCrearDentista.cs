using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista
{
    public class ComandoCrearDentista : IRequest<Guid>
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
    }
}
