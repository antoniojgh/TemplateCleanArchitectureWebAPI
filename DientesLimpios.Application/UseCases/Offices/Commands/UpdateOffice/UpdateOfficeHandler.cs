using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice
{
    public class UpdateOfficeHandler(IOfficeRepository repository, IUnitOfWork unitOfWork, ILogger<UpdateOfficeHandler> logger) : IRequestHandler<UpdateOfficeCommand, Result>
    {
        public async Task<Result> Handle(UpdateOfficeCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating office with ID: {OfficeId}", request.Id);

            var office = await repository.GetById(request.Id);

            if (office is null)
                return Result.Failure(DomainErrors.Office.NotFound);

            var actualizarResult = office.UpdateName(request.Name);
            if (actualizarResult.IsFailure)
                return actualizarResult;

            await repository.Update(office);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Office updated successfully with ID: {OfficeId}", request.Id);

            return Result.Success();

        }
    }
}
