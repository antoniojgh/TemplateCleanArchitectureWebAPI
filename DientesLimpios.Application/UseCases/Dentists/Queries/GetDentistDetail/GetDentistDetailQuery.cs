using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistDetail
{
    public class GetDentistDetailQuery : IRequest<Result<DentistDetailDTO>>
    {
        public required Guid Id { get; set; }
    }
}
