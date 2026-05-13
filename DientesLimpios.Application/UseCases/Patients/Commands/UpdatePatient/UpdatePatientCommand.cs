using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Patients.Commands.UpdatePatient
{
    public class UpdatePatientCommand : IRequest<Result>
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
