using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList
{
    public class GetOfficeListHandler(IOfficeRepository repository, ILogger<GetOfficeListHandler> logger) : IRequestHandler<GetOfficeListQuery, Result<List<OfficeListDTO>>>
    {
        public async Task<Result<List<OfficeListDTO>>> Handle(GetOfficeListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo listado de offices");

            var offices = await repository.GetAll();
            var consultoriosDTO = offices.Select(office => office.ADto()).ToList();

            logger.LogInformation("List de offices obtenido correctamente con {NumeroOffices} offices", consultoriosDTO.Count);

            return Result.Success(consultoriosDTO);
        }
    }
}
