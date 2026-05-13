using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.CreateDentist
{
    public class CreateDentistCommand : IRequest<Result<Guid>>
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
