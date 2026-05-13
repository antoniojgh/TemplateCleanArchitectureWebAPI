using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Patients.Commands.DeletePatient
{
    public class DeletePatientCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
