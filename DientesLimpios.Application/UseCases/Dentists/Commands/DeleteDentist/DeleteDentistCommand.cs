using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.DeleteDentist
{
    public class DeleteDentistCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
