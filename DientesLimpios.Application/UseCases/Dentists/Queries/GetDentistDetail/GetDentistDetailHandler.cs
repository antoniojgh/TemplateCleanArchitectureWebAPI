using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistDetail
{
    public class GetDentistDetailHandler(IApplicationDbContext db, ILogger<GetDentistDetailHandler> logger) : IRequestHandler<GetDentistDetailQuery, Result<DentistDetailDTO>>
    {
        public async Task<Result<DentistDetailDTO>> Handle(GetDentistDetailQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving dentist detail with ID: {DentistId}", request.Id);

            var dentistDTO = await db.Dentists
                .Where(d => d.Id == request.Id)
                .Select(d => new DentistDetailDTO { Id = d.Id, Name = d.Name, Email = d.Email.Value })
                .FirstOrDefaultAsync(cancellationToken);

            if (dentistDTO is null)
                return Result.Failure<DentistDetailDTO>(DomainErrors.Dentist.NotFound);

            logger.LogInformation("Dentist detail retrieved successfully with ID: {DentistId}", request.Id);

            return Result.Success(dentistDTO);
        }
    }
}
