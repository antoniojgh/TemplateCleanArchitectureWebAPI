using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice
{
    public class UpdateOfficeCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }
}
