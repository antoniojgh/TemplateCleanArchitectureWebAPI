using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Patients.Commands.CreatePatient
{
   public class CreatePatientCommand : IRequest<Result<Guid>>
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
