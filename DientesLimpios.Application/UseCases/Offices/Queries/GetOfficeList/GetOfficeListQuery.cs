using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList
{
    public class GetOfficeListQuery : IRequest<Result<List<OfficeListDTO>>>
    {
    }
}
