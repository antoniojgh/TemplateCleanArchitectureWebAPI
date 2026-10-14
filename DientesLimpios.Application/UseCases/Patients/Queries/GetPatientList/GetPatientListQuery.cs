using DientesLimpios.Application.Utilities.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public class GetPatientListQuery : IRequest<Result<PagedDTO<PatientListDTO>>>
    {
        public int Page { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
