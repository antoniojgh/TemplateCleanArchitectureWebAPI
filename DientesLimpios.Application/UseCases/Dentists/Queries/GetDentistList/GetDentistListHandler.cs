using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public class GetDentistListHandler(IDentistRepository repository, ILogger<GetDentistListHandler> logger) : IRequestHandler<GetDentistListQuery, Result<PagedDTO<DentistListDTO>>>
    {
        public async Task<Result<PagedDTO<DentistListDTO>>> Handle(GetDentistListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving dentist list");

            var filteredDentists = await repository.GetFiltered(request);
            var totalDentists = await repository.GetTotalRecordCount();

            var filteredDentistsDTO = filteredDentists.Select(dentist => dentist.ADto()).ToList();

            var dentistsDTO = new PagedDTO<DentistListDTO>
            {
                Elementos = filteredDentistsDTO,
                Total = totalDentists
            };

            logger.LogInformation("Dentist list retrieved successfully with {DentistCount} dentists", dentistsDTO.Elementos.Count);

            return Result.Success(dentistsDTO);
        }
    }
}
