using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice
{
    public class CreateOfficeCommand : IRequest<Result<Guid>>
    {
        public required string Name { get; set; }
    }
}
