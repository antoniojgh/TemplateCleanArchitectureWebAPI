using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice
{
    public class DeleteOfficeHandler(IOfficeRepository repository, IUnitOfWork unitOfWork, ILogger<DeleteOfficeHandler> logger) : IRequestHandler<DeleteOfficeCommand, Result>
    {
        public async Task<Result> Handle(DeleteOfficeCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting office with ID: {OfficeId}", request.Id);

            var office = await repository.GetById(request.Id);

            if (office is null)
                return Result.Failure(DomainErrors.Office.NotFound);

            await repository.Delete(office);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Office deleted successfully with ID: {OfficeId}", request.Id);

            return Result.Success();
        }
    }
}
