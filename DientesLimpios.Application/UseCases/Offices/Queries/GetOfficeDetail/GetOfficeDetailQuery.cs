using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeDetail
{
    public class GetOfficeDetailQuery : IRequest<Result<OfficeDetailDTO>>
    {
        public Guid Id { get; set; }
    }
}
