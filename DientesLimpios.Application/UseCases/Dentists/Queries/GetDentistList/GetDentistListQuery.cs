using DientesLimpios.Application.Utilities.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public class GetDentistListQuery : DentistFilterDTO, IRequest<Result<PagedDTO<DentistListDTO>>>
    {
    }
}
