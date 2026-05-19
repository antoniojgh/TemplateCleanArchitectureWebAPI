using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice
{
    public class DeleteOfficeHandler(IApplicationDbContext db, ILogger<DeleteOfficeHandler> logger) : IRequestHandler<DeleteOfficeCommand, Result>
    {
        public async Task<Result> Handle(DeleteOfficeCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting office with ID: {OfficeId}", request.Id);

            var office = await db.Offices.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (office is null)
                return Result.Failure(DomainErrors.Office.NotFound);

            db.Offices.Remove(office);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Office deleted successfully with ID: {OfficeId}", request.Id);

            return Result.Success();
        }
    }
}
