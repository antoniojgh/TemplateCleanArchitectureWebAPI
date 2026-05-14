using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.UpdateDentist
{
    public class UpdateDentistHandler(IDentistRepository repository, IUnitOfWork unitOfWork, ILogger<UpdateDentistHandler> logger) : IRequestHandler<UpdateDentistCommand, Result>
    {
        public async Task<Result> Handle(UpdateDentistCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating dentist with ID: {DentistId}", request.Id);

            var dentist = await repository.GetById(request.Id);

            if (dentist is null)
                return Result.Failure(DomainErrors.Dentist.NotFound);

            var actualizarNombreResult = dentist.UpdateName(request.Name);
            if (actualizarNombreResult.IsFailure)
                return actualizarNombreResult;

            var actualizarEmailResult = dentist.UpdateEmail(request.Email);
            if (actualizarEmailResult.IsFailure)
                return actualizarEmailResult;

            await repository.Update(dentist);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Dentist updated successfully with ID: {DentistId}", request.Id);

            return Result.Success();
        }
    }
}
