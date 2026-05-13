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
            logger.LogInformation("Obteniendo listado de dentists");

            var filteredDentists = await repository.GetFiltered(request);
            var totalDentists = await repository.GetTotalRecordCount();

            var filteredDentistsDTO = filteredDentists.Select(dentist => dentist.ADto()).ToList();

            var dentistasDTO = new PagedDTO<DentistListDTO>
            {
                Elementos = filteredDentistsDTO,
                Total = totalDentists
            };

            logger.LogInformation("List de dentists obtenido correctamente con {NumeroDentists} dentists", dentistasDTO.Elementos.Count);

            return Result.Success(dentistasDTO);
        }
    }
}
