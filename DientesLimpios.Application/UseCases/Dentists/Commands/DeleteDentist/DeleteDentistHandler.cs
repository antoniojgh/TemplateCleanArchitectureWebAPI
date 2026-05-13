using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.DeleteDentist
{
    public class DeleteDentistHandler(IDentistRepository repository, IUnitOfWork unitOfWork, ILogger<DeleteDentistHandler> logger) : IRequestHandler<DeleteDentistCommand, Result>
    {
        public async Task<Result> Handle(DeleteDentistCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Borrando dentist con ID: {DentistId}", request.Id);

            var dentist = await repository.GetById(request.Id);

            if (dentist is null)
                return Result.Failure(DomainErrors.Dentist.NotFound);

            await repository.Delete(dentist);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Dentist borrado correctamente con ID: {DentistId}", request.Id);

            return Result.Success();
        }
    }
}
