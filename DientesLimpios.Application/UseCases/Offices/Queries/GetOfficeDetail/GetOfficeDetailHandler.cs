using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeDetail
{
    public class GetOfficeDetailHandler(IOfficeRepository repository, ILogger<GetOfficeDetailHandler> logger) : IRequestHandler<GetOfficeDetailQuery, Result<OfficeDetailDTO>>
    {
        public async Task<Result<OfficeDetailDTO>> Handle(GetOfficeDetailQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo detalle de office con ID: {OfficeId}", request.Id);

            var office = await repository.GetById(request.Id);

            if (office is null)
                return Result.Failure<OfficeDetailDTO>(DomainErrors.Office.NotFound);

            logger.LogInformation("Detail de office obtenido correctamente con ID: {OfficeId}", request.Id);

            return Result.Success(office.ADto());
        }
    }
}
