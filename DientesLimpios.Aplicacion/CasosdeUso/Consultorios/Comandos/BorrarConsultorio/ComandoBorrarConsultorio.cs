using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio
{
    public class ComandoBorrarConsultorio : IRequest
    {
        public Guid Id { get; set; }
    }
}
