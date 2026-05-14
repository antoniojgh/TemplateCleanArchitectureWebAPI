using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistDetail
{
    public class GetDentistDetailHandler(IDentistRepository repository, ILogger<GetDentistDetailHandler> logger) : IRequestHandler<GetDentistDetailQuery, Result<DentistDetailDTO>>
    {
        public async Task<Result<DentistDetailDTO>> Handle(GetDentistDetailQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving dentist detail with ID: {DentistId}", request.Id);

            var dentist = await repository.GetById(request.Id);

            if (dentist is null)
                return Result.Failure<DentistDetailDTO>(DomainErrors.Dentist.NotFound);

            logger.LogInformation("Dentist detail retrieved successfully with ID: {DentistId}", request.Id);

            return Result.Success(dentist.ADto());
        }
    }
}
