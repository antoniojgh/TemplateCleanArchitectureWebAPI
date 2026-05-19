using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList
{
    public class GetOfficeListHandler(IApplicationDbContext db, ILogger<GetOfficeListHandler> logger) : IRequestHandler<GetOfficeListQuery, Result<List<OfficeListDTO>>>
    {
        public async Task<Result<List<OfficeListDTO>>> Handle(GetOfficeListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving office list");

            var offices = await db.Offices.AsNoTracking().ToListAsync(cancellationToken);
            var officesDTO = offices.Select(office => office.ADto()).ToList();

            logger.LogInformation("Office list retrieved successfully with {OfficeCount} offices", officesDTO.Count);

            return Result.Success(officesDTO);
        }
    }
}
