using DientesLimpios.Application.Utilities.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public class GetPatientListQuery : PatientFilterDTO, IRequest<Result<PagedDTO<PatientListDTO>>>
    {
    }
}
