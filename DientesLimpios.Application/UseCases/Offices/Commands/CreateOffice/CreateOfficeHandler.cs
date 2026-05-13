using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice
{
    public class CreateOfficeHandler(IOfficeRepository repository, IUnitOfWork unitOfWork, ILogger<CreateOfficeHandler> logger) : IRequestHandler<CreateOfficeCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateOfficeCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creando office con name: {Name}", request.Name);

            var officeResult = Office.Create(request.Name);

            if (officeResult.IsFailure)
                return Result.Failure<Guid>(officeResult.Error);

            var office = officeResult.Value;

            var response = await repository.Add(office);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Office creado correctamente con name: {Name}", request.Name);

            return Result.Success(response.Id);

        }
    }
}
