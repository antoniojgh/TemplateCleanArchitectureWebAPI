using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientDetail
{
    public class GetPatientDetailQuery : IRequest<Result<PatientDetailDTO>>
    {
        public required Guid Id { get; set; }
    }
}
