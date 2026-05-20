using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice
{
    public class UpdateOfficeHandler(IApplicationDbContext db, ILogger<UpdateOfficeHandler> logger) : IRequestHandler<UpdateOfficeCommand, Result>
    {
        public async Task<Result> Handle(UpdateOfficeCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating office with ID: {OfficeId}", request.Id);

            var office = await db.Offices.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (office is null)
                return Result.Failure(DomainErrors.Office.NotFound);

            var updateResult = office.UpdateName(request.Name);
            if (updateResult.IsFailure)
                return updateResult;

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Office updated successfully with ID: {OfficeId}", request.Id);

            return Result.Success();

        }
    }
}
