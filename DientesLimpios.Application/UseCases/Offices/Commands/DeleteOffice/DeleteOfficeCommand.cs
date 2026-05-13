using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice
{
    public class DeleteOfficeCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
